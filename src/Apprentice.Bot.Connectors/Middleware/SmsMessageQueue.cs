namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Middleware
{
    using System.Threading;
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

        public Task OnTurnAsync(
            ITurnContext turnContext,
            NextDelegate next,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new System.NotImplementedException();
        }
    }
}