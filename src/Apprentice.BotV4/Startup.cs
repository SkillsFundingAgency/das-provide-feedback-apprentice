#define TRACE
namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Interfaces;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Middleware;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Commands.Dialog;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Configuration;
    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Middleware;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Helpers;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;
    using ESFA.DAS.ProvideFeedback.Apprentice.Data.Repositories;
    using ESFA.DAS.ProvideFeedback.Apprentice.Services;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Localization;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Azure;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Integration;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Configuration;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using NLog.Extensions.Logging;
    using FeatureToggles = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Features;

    public class Startup
    {
        private bool isProduction = false;
        private ILogger<Startup> logger;
        private ILoggerFactory loggerFactory;

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder().SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            this.Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            IApplicationLifetime applicationLifetime,
            ILogger<Startup> logger,
            ILogger<FeedbackBot> botLogger,
            ILoggerFactory loggerFactory)
        {
            this.loggerFactory = this.ConfigureLoggerFactory(loggerFactory);
            this.logger = loggerFactory.CreateLogger<Startup>();
            applicationLifetime.ApplicationStarted.Register(() => logger.LogInformation("Host fully started"));
            applicationLifetime.ApplicationStopping.Register(() => logger.LogInformation("Host shutting down...waiting to complete requests."));
            applicationLifetime.ApplicationStopped.Register(() => logger.LogInformation("Host fully stopped. All requests processed."));

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles().UseStaticFiles().UseBotFramework();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // configure JSON serializer
            this.ConfigureJsonSerializer();

            // bind configuration settings
            this.ConfigureOptions(services);

            // add & configure localization
            this.ConfigureLocalization(services);

            // register services
            this.RegisterServices(services);

            // register commands. These are usually conversational interrupts
            this.RegisterAllDialogCommands(services);

            // register all surveys
            this.RegisterAllSurveys(services);

            services.AddSingleton<IFeedbackRepository, CosmosFeedbackRepository>();
            services.AddSingleton<IConversationRepository, CosmosConversationRepository>();

            string secretKey = this.Configuration.GetSection("botFileSecret")?.Value;
            string botFilePath = this.Configuration.GetSection("botFilePath")?.Value;

            // Loads .bot configuration file and adds a singleton that your Bot can access through dependency injection.
            BotConfiguration botConfig = BotConfiguration.Load(botFilePath ?? @".\BotConfiguration.bot", secretKey);
            services.AddSingleton(
                sp => botConfig ?? throw new InvalidOperationException($"The .bot config file could not be loaded."));

            // Retrieve current endpoint.
            var environment = this.isProduction ? "production" : "development";
            var service = botConfig.Services.FirstOrDefault(s => s.Type == "endpoint" && s.Name == environment);
            if (!(service is EndpointService endpointService))
            {
                throw new InvalidOperationException(
                    $"The .bot file does not contain an endpoint with name '{environment}'.");
            }

            IStorage dataStore = this.ConfigureStateDataStore(this.Configuration);

            // add & configure bot framework
            services.AddBot<FeedbackBot>(
                options =>
                {
                    options.CredentialProvider = new SimpleCredentialProvider(
                        this.Configuration["MicrosoftAppId"],
                        this.Configuration["MicrosoftAppPassword"]);

                    // Catches any errors that occur during a conversation turn and logs them.
                    options.OnTurnError = async (context, exception) =>
                    {
                        var botLogger = this.loggerFactory.CreateLogger<FeedbackBot>();
                        botLogger.LogError($"Exception caught : {exception}");
                        await context.SendActivityAsync($"Exception {exception}");
                    };

                    var conversationState = new ConversationState(dataStore);
                    options.State.Add(conversationState);

                    var userState = new UserState(dataStore);
                    options.State.Add(userState);

                    // options.Middleware.Add(new ConversationState<ConversationInfo>(dataStore));
                    // options.Middleware.Add(new UserState<UserProfile>(dataStore));
                    // options.Middleware.Add<AzureStorageSmsRelay>(services);
                    options.Middleware.Add<ChannelConfigurationMiddleware>(services);
                    options.Middleware.Add<ConversationLogMiddleware>(services);
                    options.Middleware.Add<IMessageQueueMiddleware>(services);
                });

            services.AddSingleton<FeedbackBotStateRepository>(
                sp =>
                {
                    // We need to grab the conversationState we added on the options in the previous step
                    var options = sp.GetRequiredService<IOptions<BotFrameworkOptions>>().Value;
                    if (options == null)
                    {
                        throw new InvalidOperationException(
                            "BotFrameworkOptions must be configured prior to setting up the State accessors");
                    }

                    var conversationState = options.State.OfType<ConversationState>().FirstOrDefault();
                    if (conversationState == null)
                    {
                        throw new InvalidOperationException(
                            "ConversationState must be defined and added before adding conversation-scoped state accessors.");
                    }

                    var userState = options.State.OfType<UserState>().FirstOrDefault();
                    if (userState == null)
                    {
                        throw new InvalidOperationException(
                            "UserState must be defined and added before adding user-scoped state accessors.");
                    }

                    // Create the custom state accessor.
                    // State accessors enable other components to read and write individual properties of state.
                    var feedbackBotState = new FeedbackBotStateRepository(conversationState, userState)
                    {
                        ConversationDialogState = conversationState.CreateProperty<DialogState>("DialogState"),
                        UserProfile = userState.CreateProperty<UserProfile>("UserProfile"),
                    };

                    return feedbackBotState;
                });
        }

        private void ConfigureJsonSerializer()
        {
            // *** WARNING: Do not use a CamelCasePropertyNamesContractResolver here - it breaks the bot session objects! ***
            JsonConvert.DefaultSettings = () =>
            {
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.Formatting = Formatting.Indented;
                settings.Converters.Add(new StringEnumConverter { CamelCaseText = true });
                return settings;
            };
        }

        private void ConfigureOptions(IServiceCollection services)
        {
            services.AddOptions()
                .BindConfiguration<Azure>(this.Configuration)
                .BindConfiguration<Bot>(this.Configuration)
                .BindConfiguration<ConnectionStrings>(this.Configuration)
                .BindConfiguration<Data>(this.Configuration)
                .BindConfiguration<Notify>(this.Configuration)
                .BindConfiguration<FeatureToggles>(this.Configuration);
        }

        private void ConfigureLocalization(IServiceCollection services)
        {
            services.AddLocalization(options => options.ResourcesPath = "Resources").Configure<RequestLocalizationOptions>(
                options =>
                {
                    var supportedCultures = new[] { new CultureInfo("en-GB") };
                    options.DefaultRequestCulture = new RequestCulture("en-GB");
                    options.SupportedCultures = supportedCultures;
                    options.SupportedUICultures = supportedCultures;
                });
        }

        private void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<IDialogFactory, DialogFactory>();
            services.AddSingleton<ISmsQueueProvider, AzureServiceBusClient>();
            services.AddSingleton<IMessageQueueMiddleware, AzureServiceBusSmsRelay>();
            services.AddTransient<ChannelConfigurationMiddleware>();
            services.AddTransient<ConversationLogMiddleware>();
        }

        private void RegisterAllSurveys(IServiceCollection services)
        {
            services.RegisterAllTypes<ISurvey>(new Assembly[] { typeof(FeedbackBot).Assembly }, ServiceLifetime.Singleton);
        }

        private void RegisterAllDialogCommands(IServiceCollection services)
        {
            services.RegisterAllTypes<IBotDialogCommand>(new Assembly[] { typeof(FeedbackBot).Assembly });
        }

        private AzureBlobStorage ConfigureStateDataStore(IConfiguration configuration)
        {
            AzureBlobStorage azureBlobStorage = new AzureBlobStorage(
                configuration["ConnectionStrings:StorageAccount"],
                configuration["Data:SessionStateTable"]);
            return azureBlobStorage;
        }

        private ILoggerFactory ConfigureLoggerFactory(ILoggerFactory loggerFactory)
        {
            loggerFactory.AddNLog(new NLogProviderOptions { CaptureMessageProperties = true, CaptureMessageTemplates = true });
            NLog.LogManager.LoadConfiguration("nlog.config");

            return loggerFactory;
        }

    }
}