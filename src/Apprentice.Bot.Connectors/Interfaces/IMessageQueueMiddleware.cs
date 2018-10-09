namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Middleware
{
    using System.Threading.Tasks;

    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;

    public interface IMessageQueueMiddleware : IMiddleware
    {
        /// <summary>
        /// Adds a message to the queue, ready for processing </summary>
        /// <param name="context"> The <see cref="ITurnContext" /> of the conversation turn/> </param>
        /// <param name="activity"> The <see cref="Activity"/> that will be used for the queue message </param>
        /// <returns> The <see cref="Task"/>. </returns>
        Task EnqueueMessageAsync(ITurnContext context, Activity activity);
    }
}