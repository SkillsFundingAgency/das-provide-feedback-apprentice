using System.Data;
using System.Data.SqlClient;
using System.IO;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.Interfaces;
using ESFA.DAS.ProvideFeedback.Apprentice.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2;
using ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.DependecyInjection.Config;
using ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.Services;
using ESFA.DAS.ProvideFeedback.Apprentice.Services;
using ESFA.DAS.ProvideFeedback.Apprentice.Services.FeedbackService.Commands.SendSms;
using ESFA.DAS.ProvideFeedback.Apprentice.Services.FeedbackService.Commands.TriggerSurveyInvites;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.Targets;
using SFA.DAS.NLog.Targets.Redis.DotNetCore;
using LogLevel = NLog.LogLevel;

[assembly: WebJobsStartup(typeof(Startup))]

namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2
{
    internal class Startup : IWebJobsStartup
    {
        private readonly IConfigurationRoot _configuration;

        public Startup()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }

        public void Configure(IWebJobsBuilder builder) => builder.AddDependencyInjection(ConfigureServices);

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IDbConnection>(c => new SqlConnection(_configuration.GetConnectionStringOrSetting("SqlConnectionString")));

            services.AddLogging((options) =>
            {
                options.AddConfiguration(_configuration.GetSection("Logging"));
                options.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                options.AddNLog(new NLogProviderOptions
                {
                    CaptureMessageTemplates = true,
                    CaptureMessageProperties = true
                });
                options.AddConsole();
                options.AddDebug();

                ConfigureNLog();
            });

            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

            services.AddScoped<IStoreApprenticeSurveyDetails, ApprenticeSurveyInvitesRepository>();
            services.AddSingleton(service => new SettingsProvider(_configuration));

            services.AddTransient<IDbConnection>(c => new SqlConnection(_configuration.GetConnectionStringOrSetting("SqlConnectionString")));
            services.AddScoped<IStoreApprenticeSurveyDetails, ApprenticeSurveyInvitesRepository>();
            services.AddScoped<IConversationRepository, ConversationRepository>();
            services.AddSingleton<IQueueClientFactory, QueueClientFactory>();

            services.AddFunctionSupport(a => a.UseDistributedLockManager(l => new AzureDistributedLockProvider(_configuration.GetConnectionStringOrSetting("AzureWebJobsStorage"), l.GetRequiredService<ILoggerFactory>(), "sms-feedback-locks")));

            services.AddTransient<ISettingService, SettingsProvider>((provider) => new SettingsProvider(_configuration));
            services.AddTransient((provider) => new Notify.Client.NotificationClient(provider.GetRequiredService<ISettingService>().Get("NotifyClientApiKey")));
            services.AddTransient<INotificationClient, NotificationClient>();

            services.AddTransient<ICommandHandlerAsync<TriggerSurveyInvitesCommand>, TriggerSurveyInvitesCommandHandler>();
            services.AddTransient<ICommandHandlerAsync<SendSmsCommand>, SendSmsCommandHandler>();
            services.Decorate<ICommandHandlerAsync<SendSmsCommand>, SendSmsCommandHandlerWithWaitForPreviousSms>();
            services.Decorate<ICommandHandlerAsync<SendSmsCommand>>((inner, provider) => new SendSmsCommandHandlerWithOrderCheck(inner, provider.GetRequiredService<IConversationRepository>()));
            services.Decorate<ICommandHandlerAsync<SendSmsCommand>>((inner, provider) => new SendSmsCommandHandlerWithLocking(inner, provider.GetRequiredService<IDistributedLockProvider>()));
            services.Decorate<ICommandHandlerAsync<SendSmsCommand>>((inner, provider) => new SendSmsCommandHandlerWithDelayHandler(inner, provider.GetRequiredService<IQueueClientFactory>(), provider.GetRequiredService<ILoggerFactory>(), provider.GetRequiredService<ISettingService>()));
        }

        private void ConfigureNLog()
        {
            var appName = "das-apprentice-survey-bot";
            var localLogPath = _configuration.GetConnectionStringOrSetting("LogDir");
            var env = _configuration.GetConnectionStringOrSetting("ASPNETCORE_ENVIRONMENT");
            var config = new LoggingConfiguration();

            if (string.IsNullOrEmpty(env))
            {
                AddLocalTarget(config, localLogPath, appName);
            }
            else
            {
                AddRedisTarget(config, appName, env);
            }

            LogManager.Configuration = config;
        }

        private static void AddLocalTarget(LoggingConfiguration config, string localLogPath, string appName)
        {
            InternalLogger.LogFile = Path.Combine(localLogPath, $"{appName}\\nlog-internal.{appName}.log");
            var fileTarget = new FileTarget("Disk")
            {
                FileName = Path.Combine(localLogPath, $"{appName}\\{appName}.${{shortdate}}.log"),
                Layout = "${longdate} [${uppercase:${level}}] [${logger}] - ${message} ${onexception:${exception:format=tostring}}"
            };
            config.AddTarget(fileTarget);

            config.AddRule(GetMinLogLevel(), LogLevel.Fatal, "Disk");
        }

        private static void AddRedisTarget(LoggingConfiguration config, string appName, string environment)
        {
            var target = new RedisTarget
            {
                Name = "RedisLog",
                AppName = appName,
                EnvironmentKeyName = "ASPNETCORE_ENVIRONMENT",
                ConnectionStringName = "Redis",
                IncludeAllProperties = true,
                Layout = "${message}"
            };

            config.AddTarget(target);
            config.AddRule(GetMinLogLevel(), LogLevel.Fatal, "RedisLog");
        }

        private static LogLevel GetMinLogLevel() => LogLevel.FromString("Info");
    }
}