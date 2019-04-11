using ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2;

using Microsoft.Azure.WebJobs.Hosting;

[assembly: WebJobsStartup(typeof(Startup))]

namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2
{
    using System.Data;
    using System.Data.SqlClient;
    using System.IO;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Interfaces;
    using ESFA.DAS.ProvideFeedback.Apprentice.Data.Repositories;
    using ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.Application.CommandHandlers;
    using ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.Application.Commands;
    using ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.DependecyInjection.Config;
    using ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.Services;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    internal class Startup : IWebJobsStartup
    {
        private readonly IConfigurationRoot configuration;

        public Startup()
        {
            this.configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }

        public void Configure(IWebJobsBuilder builder) => builder.AddDependencyInjection(this.ConfigureServices);

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IDbConnection>(c => new SqlConnection(this.configuration.GetConnectionStringOrSetting("SqlConnectionString")));
            services.AddScoped<IStoreApprenticeSurveyDetails, ApprenticeSurveyInvitesRepository>();
            services.AddScoped<IConversationRepository, ConversationRepository>();
            services.AddSingleton<IQueueClient>(new QueueClient(this.configuration.GetConnectionStringOrSetting("ServiceBusConnection"), "sms-outgoing-messages"));

            services.AddLogging();
            services.AddFunctionSupport(a => a.UseDistributedLockManager(l => new AzureDistributedLockProvider(this.configuration.GetConnectionStringOrSetting("AzureWebJobsStorage"), l.GetService<ILoggerFactory>(), "sms-feedback-locks")));

            services.AddTransient<ISettingService, SettingsProvider>((provider) => new SettingsProvider(configuration));
            services.AddTransient((provider) => new Notify.Client.NotificationClient(provider.GetService<ISettingService>().Get("NotifyClientApiKey")));
            services.AddTransient<INotificationClient, NotificationClient>();

            services.AddTransient<ICommandHandlerAsync<SendSmsCommand>, SendSmsCommandHandler>();
            services.Decorate<ICommandHandlerAsync<SendSmsCommand>, SendSmsCommandHandlerWithWaitForPreviousSms>();
            services.Decorate<ICommandHandlerAsync<SendSmsCommand>>((inner, provider) => new SendSmsCommandHandlerWithOrderCheck(inner, provider.GetRequiredService<IConversationRepository>()));
            services.Decorate<ICommandHandlerAsync<SendSmsCommand>>((inner, provider) => new SendSmsCommandHandlerWithLocking(inner, provider.GetRequiredService<IDistributedLockProvider>()));
            services.Decorate<ICommandHandlerAsync<SendSmsCommand>>((inner, provider) => new SendSmsCommandHandlerWithDelayHandler(inner, provider.GetRequiredService<IQueueClient>(), provider.GetRequiredService<ILoggerFactory>(), provider.GetRequiredService<ISettingService>()));
        }
    }
}