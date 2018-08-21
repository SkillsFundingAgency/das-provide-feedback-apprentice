namespace ESFA.ProvideFeedback.Apprentice.NotifyMessageHandler
{
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Configuration;

    public class SettingsProvider : ISettingService
    {
        private readonly IConfigurationRoot _config;

        public SettingsProvider(ExecutionContext ctx)
        {
            this._config = new ConfigurationBuilder()
                .SetBasePath(ctx.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }

        public string Get(string parameterName)
        {
            var parameter = this._config[parameterName];
            return parameter;
        }

        public SettingsProvider Current => this;
    }
}