namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.Services
{
    using System;
    using Microsoft.Extensions.Configuration;

    public class SettingsProvider : ISettingService
    {
        private readonly IConfigurationRoot configuration;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SettingsProvider"/> class.
        /// </summary>
        /// <param name="ctx"> The <see cref="IConfigurationRoot"/>. </param>
        public SettingsProvider(IConfigurationRoot configRoot)
        {
            this.configuration = configRoot;
        }

        public string Get(string parameterName)
        {
            var parameter = this.configuration[parameterName];
            return parameter;
        }

        public int GetInt(string parameterName)
        {
            string parameter = this.Get(parameterName);
            if (!int.TryParse(parameter, out int result))
            {
                throw new Exception($"Configuration variable {parameterName} could not be cast to an integer");

            }
            return result;
        }
    }
}