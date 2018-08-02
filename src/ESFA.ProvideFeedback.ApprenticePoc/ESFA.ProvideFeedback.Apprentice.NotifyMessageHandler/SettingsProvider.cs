using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;

namespace ESFA.ProvideFeedback.Apprentice.NotifyMessageHandler
{
    public class SettingsProvider : ISettingService
    {
        private readonly IConfigurationRoot _config;

        public SettingsProvider(ExecutionContext ctx)
        {
            _config = new ConfigurationBuilder()
                .SetBasePath(ctx.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }

        public string Get(string parameterName)
        {
            var parameter = _config[parameterName];
            return parameter;
        }
    }
}