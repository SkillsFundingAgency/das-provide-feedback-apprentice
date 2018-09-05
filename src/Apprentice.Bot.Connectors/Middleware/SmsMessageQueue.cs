namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Middleware
{
    using System.Threading.Tasks;

    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;

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
}