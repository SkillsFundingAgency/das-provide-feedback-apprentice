using System.Reflection;
using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Commands;
using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Middleware;
using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Dialogs;
using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Middleware;
using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.Helpers;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;

namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4
{
    using System;
    using System.Globalization;
    using System.IO;
    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Configuration;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Localization;
    using Microsoft.Bot.Builder.Azure;
    using Microsoft.Bot.Builder.BotFramework;
    using Microsoft.Bot.Builder.Core.Extensions;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Builder.TraceExtensions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    using AzureSettings = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Azure;
    using BotSettings = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Bot;
    using DataSettings = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Data;
    using NotifySettings = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Notify;
    using FeatureToggles = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Features;

    public class Startup
    {
        private ILoggerFactory _loggerFactory;

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
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
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
            services.AddSingleton<IDialogFactory, DialogFactory>()
                    .AddSingleton<IMessageQueueMiddleware, AzureStorageQueueSmsRelay>()
                    .AddSingleton<ISmsQueueProvider, AzureStorageSmsQueueClient>()
                    //.AddTransient<ILogger>(provider => _loggerFactory.CreateLogger<FeedbackBot>())
                    .RegisterAllTypes<IBotDialogCommand>(new[] { typeof(IBotDialogCommand).Assembly }, ServiceLifetime.Transient)
                    .RegisterAllTypes<ISurvey>(new[] { typeof(ISurvey).Assembly }, ServiceLifetime.Transient);

            // add & configure bot framework
            services.AddBot<FeedbackBot>(
                options =>
                    {
                        //ILogger logger = _loggerFactory.CreateLogger<FeedbackBot>();

                        options.CredentialProvider = new ConfigurationCredentialProvider(this.Configuration);
                        options.Middleware.Add(
                            new CatchExceptionMiddleware<Exception>(
                                async (context, exception) =>
                                    {
                                        await context.TraceActivity($"{nameof(FeedbackBot)} Exception", exception);
                                        //logger.LogError(exception, $"{nameof(FeedbackBot)} Exception");
#if DEBUG
                                        await context.SendActivity($"Sorry, it looks like something went wrong! {exception.Message}");
#endif
                                    }));

                        IStorage dataStore = ConfigureStateDataStore(this.Configuration);

                        options.Middleware.Add(new ConversationState<ConversationInfo>(dataStore));
                        options.Middleware.Add(new UserState<UserInfo>(dataStore));
                        //options.Middleware.Add<AzureStorageQueueSmsRelay>(services);
                        options.Middleware.Add<IMessageQueueMiddleware>(services);
                    });
        }

        private static AzureBlobStorage ConfigureStateDataStore(IConfiguration configuration)
        {
            AzureBlobStorage azureBlobStorage = new AzureBlobStorage(
                configuration["ConnectionStrings:StorageAccount"],
                configuration["Data:SessionStateTable"]);
            return azureBlobStorage;
        }
    }
}