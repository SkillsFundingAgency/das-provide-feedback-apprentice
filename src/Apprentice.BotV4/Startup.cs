using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Commands;
using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Middleware;
using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.Helpers;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;
using Microsoft.AspNetCore.Localization;
using Microsoft.Bot.Builder.Azure;

namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4
{
    using System;
    using System.Globalization;
    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Configuration;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration;
    using System.Linq;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
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

    using AzureSettings = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Azure;
    using BotSettings = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Bot;
    using DataSettings = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Data;
    using NotifySettings = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Notify;
    using FeatureToggles = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Features;

    public class Startup
    {
        private ILoggerFactory loggerFactory;

        private bool isProduction = false;

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder().SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true).AddEnvironmentVariables();

            this.Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles().UseStaticFiles().UseBotFramework();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // configure JSON serializer
            // *** WARNING: Do not use a CamelCasePropertyNamesContractResolver here - it breaks the bot session objects! ***
            JsonConvert.DefaultSettings = () =>
                {
                    JsonSerializerSettings settings = new JsonSerializerSettings();
                    settings.Formatting = Formatting.Indented;
                    settings.Converters.Add(new StringEnumConverter { CamelCaseText = true });
                    return settings;
                };

            // bind configuration settings
            services.AddOptions()
                .BindConfiguration<AzureSettings>(this.Configuration)
                .BindConfiguration<BotSettings>(this.Configuration)
                .BindConfiguration<ConnectionStrings>(this.Configuration)
                .BindConfiguration<DataSettings>(this.Configuration)
                .BindConfiguration<NotifySettings>(this.Configuration)
                .BindConfiguration<FeatureToggles>(this.Configuration);

            // add & configure localization
            services.AddLocalization(options => options.ResourcesPath = "Resources")
                .Configure<RequestLocalizationOptions>(
                    options =>
                        {
                            var supportedCultures = new[] { new CultureInfo("en-GB") };
                            options.DefaultRequestCulture = new RequestCulture("en-GB");
                            options.SupportedCultures = supportedCultures;
                            options.SupportedUICultures = supportedCultures;
                        });

            // add & register services
            //services.AddTransient<ILogger>(provider => loggerFactory.CreateLogger<FeedbackBot>());

            // services.AddSingleton<IDialogFactory, DialogFactory>();
            //services.AddSingleton<IMessageQueueMiddleware, AzureServiceBusQueueSmsRelay>();
            services.AddSingleton<IMessageQueueMiddleware, AzureStorageQueueSmsRelay>();
            services.AddSingleton<ISmsQueueProvider, AzureStorageSmsQueueClient>();

            services.RegisterAllTypes<IBotDialogCommand>(new[] { typeof(IBotDialogCommand).Assembly }, ServiceLifetime.Transient);
            services.RegisterAllTypes<ISurvey>(new[] { typeof(ISurvey).Assembly }, ServiceLifetime.Transient);


            string secretKey = this.Configuration.GetSection("botFileSecret")?.Value;
            string botFilePath = this.Configuration.GetSection("botFilePath")?.Value;

            // Loads .bot configuration file and adds a singleton that your Bot can access through dependency injection.
            BotConfiguration botConfig = BotConfiguration.Load(botFilePath ?? @".\BotConfiguration.bot", secretKey);
            services.AddSingleton(sp => botConfig ?? throw new InvalidOperationException($"The .bot config file could not be loaded."));

            // Add BotServices singleton.
            // Create the connected services from .bot file.
            // services.AddSingleton(sp => new BotServices(botConfig));

            // Retrieve current endpoint.
            var environment = this.isProduction ? "production" : "development";
            var service = botConfig.Services.FirstOrDefault(s => s.Type == "endpoint" && s.Name == environment);
            if (!(service is EndpointService endpointService))
            {
                throw new InvalidOperationException($"The .bot file does not contain an endpoint with name '{environment}'.");
            }

            // // Storage configuration name or ID from the .bot file.
            // const string StorageConfigurationId = "<STORAGE-NAME-OR-ID-FROM-BOT-FILE>";
            // var blobConfig = botConfig.FindServiceByNameOrId(StorageConfigurationId);
            // if (!(blobConfig is BlobStorageService blobStorageConfig))
            // {
            //    throw new InvalidOperationException($"The .bot file does not contain an blob storage with name '{StorageConfigurationId}'.");
            // }
            // // Default container name.
            // const string DefaultBotContainer = "<DEFAULT-CONTAINER>";
            // var storageContainer = string.IsNullOrWhiteSpace(blobStorageConfig.Container) ? DefaultBotContainer : blobStorageConfig.Container;
            // IStorage dataStore = new Microsoft.Bot.Builder.Azure.AzureBlobStorage(blobStorageConfig.ConnectionString, storageContainer);

            IStorage dataStore = this.ConfigureStateDataStore(this.Configuration);

            // add & configure bot framework
            services.AddBot<FeedbackBot>(
                options =>
                    {
                        options.CredentialProvider = new SimpleCredentialProvider(endpointService.AppId, endpointService.AppPassword);


                        ILogger logger = this.loggerFactory.CreateLogger<FeedbackBot>();

                        // Catches any errors that occur during a conversation turn and logs them.
                        options.OnTurnError = async (context, exception) =>
                            {
                                logger.LogError($"Exception caught : {exception}");
                                await context.SendActivityAsync("Sorry, it looks like something went wrong.");
                            };

                        var conversationState = new ConversationState(dataStore);
                        options.State.Add(conversationState);

                        var userState = new UserState(dataStore);
                        options.State.Add(userState);

                        // options.Middleware.Add(new ConversationState<ConversationInfo>(dataStore));
                        // options.Middleware.Add(new UserState<UserInfo>(dataStore));
                        //options.Middleware.Add<AzureStorageQueueSmsRelay>(services);
                        //options.Middleware.Add<IMessageQueueMiddleware>(services);
                    });

            services.AddSingleton<FeedbackBotState>(sp =>
            {
                // We need to grab the conversationState we added on the options in the previous step
                var options = sp.GetRequiredService<IOptions<BotFrameworkOptions>>().Value;
                if (options == null)
                {
                    throw new InvalidOperationException("BotFrameworkOptions must be configured prior to setting up the State Accessors");
                }
                var conversationState = options.State.OfType<ConversationState>().FirstOrDefault();
                if (conversationState == null)
                {
                    throw new InvalidOperationException("ConversationState must be defined and added before adding conversation-scoped state accessors.");
                }

                var userState = options.State.OfType<UserState>().FirstOrDefault();
                if (userState == null)
                {
                    throw new InvalidOperationException("UserState must be defined and added before adding user-scoped state accessors.");
                }

                // Create the custom state accessor.
                // State accessors enable other components to read and write individual properties of state.
                var feedbackBotState = new FeedbackBotState(conversationState, userState)
                {
                    ConversationDialogState = conversationState.CreateProperty<DialogState>("DialogState"),
                    UserInfo = userState.CreateProperty<UserInfo>("UserInfo"),
                };

                return feedbackBotState;
            });
        }

        private AzureBlobStorage ConfigureStateDataStore(IConfiguration configuration)
        {
            AzureBlobStorage azureBlobStorage = new AzureBlobStorage(
                configuration["ConnectionStrings:StorageAccount"],
                configuration["Data:SessionStateTable"]);
            return azureBlobStorage;
        }
    }
}