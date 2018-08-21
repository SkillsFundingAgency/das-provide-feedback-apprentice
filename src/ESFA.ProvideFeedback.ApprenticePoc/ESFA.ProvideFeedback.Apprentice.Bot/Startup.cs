// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Startup.cs" company="Education & Skills Funding Agency">
//   Copyright © 2018 Education & Skills Funding Agency
// </copyright>
// <summary>
//   The program bootstrapper.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ESFA.ProvideFeedback.Apprentice.Bot
{
    using System;
    using System.Globalization;

    using ESFA.ProvideFeedback.Apprentice.Bot.Config;
    using ESFA.ProvideFeedback.Apprentice.Bot.Dto;
    using ESFA.ProvideFeedback.Apprentice.Bot.Helpers;
    using ESFA.ProvideFeedback.Apprentice.Bot.Middleware;
    using ESFA.ProvideFeedback.Apprentice.Bot.Models;
    using ESFA.ProvideFeedback.Apprentice.Bot.Services;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Localization;
    using Microsoft.Bot.Builder.Azure;
    using Microsoft.Bot.Builder.BotFramework;
    using Microsoft.Bot.Builder.Core.Extensions;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Builder.TraceExtensions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    using NLog;
    using NLog.Web;

    /// <summary>
    /// The program bootstrapper.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="env">
        /// The hosting environment.
        /// </param>
        public Startup(IHostingEnvironment env)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true).AddEnvironmentVariables();

            this.Configuration = builder.Build();
        }

        /// <summary>
        /// Gets the configuration provider.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">the application builder</param>
        /// <param name="env">the hosting environment</param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles().UseStaticFiles().UseBotFramework();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        /// </summary>
        /// <param name="services">a collection of services that can be used throughout the lifecycle of the program</param>
        public void ConfigureServices(IServiceCollection services)
        {
            Logger logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

            JsonConvert.DefaultSettings = () =>
                {
                    JsonSerializerSettings settings = new JsonSerializerSettings();
                    settings.Converters.Add(new StringEnumConverter { CamelCaseText = true });
                    return settings;
                };

            // add config options
            services.AddOptions().BindConfiguration<Azure>(this.Configuration)
                .BindConfiguration<Bot>(this.Configuration).BindConfiguration<ConnectionStrings>(this.Configuration)
                .BindConfiguration<Data>(this.Configuration).BindConfiguration<Notify>(this.Configuration);

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

            // add services
            services.AddSingleton<IApprenticeFeedbackResources, ApprenticeFeedbackResourcesV3>()
                .AddTransient<IApprenticeFeedbackSurvey, ApprenticeFeedbackV3>()
                .AddSingleton<IDialogFactory<DialogSet>, BotDialogFactory>()
                .AddSingleton<IConversationLogMiddleware, CosmosConversationLog>()
                .AddSingleton<ISmsRelayMiddleware, AzureStorageQueueSmsRelay>();

            // add & configure bot framework
            services.AddBot<FeedbackBot>(
                options =>
                    {
                        options.CredentialProvider = new ConfigurationCredentialProvider(this.Configuration);

                        // The CatchExceptionMiddleware provides a top-level exception handler for your bot. 
                        // Any exceptions thrown by other Middleware, or by your OnTurn method, will be 
                        // caught here. To facilitate debugging, the exception is sent out, via Trace, 
                        // to the emulator. Trace activities are NOT displayed to users, so in addition
                        // an "Oops" message is sent. 
                        options.Middleware.Add(
                            new CatchExceptionMiddleware<Exception>(
                                async (context, exception) =>
                                    {
                                        await context.TraceActivity($"{nameof(FeedbackBot)} Exception", exception);
                                        logger.Error(exception, $"{nameof(FeedbackBot)} Exception");
                                        await context.SendActivity(exception.Message);
                                    }));

                        // The Memory Storage used here is for local bot debugging only. When the bot
                        // is restarted, anything stored in memory will be gone. 
                        // IStorage dataStore = new MemoryStorage();

                        // The File data store, shown here, is suitable for bots that run on 
                        // a single machine and need durable state across application restarts.                 
                        // IStorage dataStore = new FileStorage(System.IO.Path.GetTempPath());

                        // For production bots use the Azure Table Store, Azure Blob, or 
                        // Azure CosmosDB storage provides, as seen below. To include any of 
                        // the Azure based storage providers, add the Microsoft.Bot.Builder.Azure 
                        // Nuget package to your solution. That package is found at:
                        // https://www.nuget.org/packages/Microsoft.Bot.Builder.Azure/

                        // IStorage dataStore = new AzureTableStorage(Configuration.GetConnectionString("StorageAccount"), "ApprenticeFeedbackBotSessions");

                        // TODO: build a bot middleware factory
                        IStorage dataStore = ConfigureStateDataStore(this.Configuration);
                        options.Middleware.Add(new ConversationState<SurveyState>(dataStore));
                        options.Middleware.Add(new UserState<UserState>(dataStore));
                        options.Middleware.Add<IConversationLogMiddleware>(services);
                        options.Middleware.Add<ISmsRelayMiddleware>(services);
                    });
        }

        /// <summary>
        /// Configure the data store for session state
        /// </summary>
        /// <param name="configuration"> The <see cref="IConfiguration"/> provider </param>
        /// <returns>
        /// the <see cref="IStorage"/>
        /// </returns>
        private static AzureBlobStorage ConfigureStateDataStore(IConfiguration configuration)
        {
            AzureBlobStorage azureBlobStorage = new AzureBlobStorage(
                configuration["ConnectionStrings:StorageAccount"],
                configuration["Data:SessionStateTable"]);
            return azureBlobStorage;
        }
    }
}