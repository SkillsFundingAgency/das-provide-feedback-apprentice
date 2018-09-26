namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Middleware
{
    using System.Threading.Tasks;

    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;

    public class SmsMessageQueue : IMessageQueueMiddleware
    {
        private readonly ISmsQueueProvider smsQueueProvider;

        public SmsMessageQueue(ISmsQueueProvider smsQueueProvider)
        {
            this.smsQueueProvider = smsQueueProvider;
        }

        public Task EnqueueMessageAsync(ITurnContext context, Activity activity)
        {
            throw new System.NotImplementedException();
        }

        public Task OnTurn(ITurnContext context, MiddlewareSet.NextDelegate next)
        {
            throw new System.NotImplementedException();
        }
    }
}