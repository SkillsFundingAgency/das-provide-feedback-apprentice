namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions
{
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Configuration;

    public class SettingsProvider : ISettingService
    {
        private readonly IConfigurationRoot config;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SettingsProvider"/> class.
        /// </summary>
        /// <param name="ctx"> The <see cref="ExecutionContext"/>. </param>
        public SettingsProvider(ExecutionContext ctx)
        {
            this.config = new ConfigurationBuilder().SetBasePath(ctx.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true).AddEnvironmentVariables()
                .Build();
        }

        public string Get(string parameterName)
        {
            var parameter = this.config[parameterName];
            return parameter;
        }
    }
}