namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Middleware
{
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Infrastructure.Configuration;

    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;

    public interface IMessageQueueMiddleware
    {
        /// <summary>
        /// Adds a message to the queue, ready for processing </summary>
        /// <param name="context"> The <see cref="ITurnContext" /> of the conversation turn/> </param>
        /// <param name="activity"> The <see cref="Activity"/> that will be used for the queue message </param>
        /// <returns> The <see cref="Task"/>. </returns>
        Task EnqueueMessageAsync(ITurnContext context, Activity activity);
    }

    public class SmsMessageQueue : IMessageQueueMiddleware
    {
        private readonly IQueueProvider queueProvider;

        public SmsMessageQueue(IQueueProvider queueProvider)
        {
            this.queueProvider = queueProvider;
        }

        public Task EnqueueMessageAsync(ITurnContext context, Activity activity)
        {
            throw new System.NotImplementedException();
        }
    }

    public interface IQueueProvider
    {
        void Send(object message);

        Task SendAsync(object message);
    }

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