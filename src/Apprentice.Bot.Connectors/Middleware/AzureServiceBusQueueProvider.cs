namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Middleware
{
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration;

    public class AzureServiceBusQueueProvider : IQueueProvider
    {
        private Azure azureConfig;

        public AzureServiceBusQueueProvider(Azure azureConfig)
        {
            this.azureConfig = azureConfig;
        }

        public void Send(object message)
        {
            throw new System.NotImplementedException();
        }

        public Task SendAsync(object message)
        {
            throw new System.NotImplementedException();
        }
    }
}