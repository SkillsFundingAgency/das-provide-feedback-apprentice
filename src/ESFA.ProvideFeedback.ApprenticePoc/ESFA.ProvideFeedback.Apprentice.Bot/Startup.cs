using System;
using ESFA.ProvideFeedback.Apprentice.Bot.Config;
using ESFA.ProvideFeedback.Apprentice.Bot.Middleware;
using ESFA.ProvideFeedback.Apprentice.Bot.Models;
using ESFA.ProvideFeedback.Apprentice.Bot.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NLog.Web;

namespace ESFA.ProvideFeedback.Apprentice.Bot
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

            services.Configure<IOptions<BotConfig>>(options => Configuration.GetSection("FeedbackBot").Bind(options));
            services.Configure<IOptions<BotConfig>>(options => Configuration.GetSection("Azure").Bind(options));
            services.Configure<IOptions<BotConfig>>(options => Configuration.GetSection("Data").Bind(options));

            services.AddTransient<IApprenticeFeedbackSurvey, ApprenticeFeedbackSurvey>();
            services.AddSingleton<IDialogFactory<DialogSet>, BotDialogFactory>();

            services.AddBot<FeedbackBot>(options =>
            {
                options.CredentialProvider = new ConfigurationCredentialProvider(Configuration);

                // The CatchExceptionMiddleware provides a top-level exception handler for your bot. 
                // Any exceptions thrown by other Middleware, or by your OnTurn method, will be 
                // caught here. To facillitate debugging, the exception is sent out, via Trace, 
                // to the emulator. Trace activities are NOT displayed to users, so in addition
                // an "Ooops" message is sent. 
                options.Middleware.Add(new CatchExceptionMiddleware<Exception>(async (context, exception) =>
                {
                    await context.TraceActivity($"{nameof(FeedbackBot)} Exception", exception);
                    logger.Error(exception, $"{nameof(FeedbackBot)} Exception");
                    await context.SendActivity("I'm sorry, but something went wrong there.");
                }));

                var azureConfig = Configuration.GetSection("Azure").Get<AzureConfig>();
                var dataConfig = Configuration.GetSection("Data").Get<DataConfig>();

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
                //      https://www.nuget.org/packages/Microsoft.Bot.Builder.Azure/

                // IStorage dataStore = new AzureTableStorage(Configuration.GetConnectionString("StorageAccount"), "ApprenticeFeedbackBotSessions");
                IStorage dataStore = new Microsoft.Bot.Builder.Azure.AzureBlobStorage(Configuration.GetConnectionString("StorageAccount"), "feedback-bot-sessions");

                options.Middleware.Add(new ConversationState<SurveyState>(dataStore));
                options.Middleware.Add(new UserState<UserState>(dataStore));
                options.Middleware.Add(new ConversationLogMiddleware(new OptionsWrapper<AzureConfig>(azureConfig), new OptionsWrapper<DataConfig>(dataConfig)));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseBotFramework();
        }
    }

}
