namespace ESFA.ProvideFeedback.Apprentice.Bot.Middleware
{
    using System.Threading.Tasks;

    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;

    /// <inheritdoc />
    /// <summary>
    /// Defines middleware to write conversation responses to an SMS relay.
    /// </summary>
    public interface ISmsRelayMiddleware : IMiddleware
    {
        /// <summary>
        /// Adds a message on to the Azure Storage Queue, ready for processing </summary>
        /// <param name="context"> The <see cref="ITurnContext" /> of the conversation turn/> </param>
        /// <param name="activity"> The <see cref="Activity"/> that will be used for the queue message </param>
        /// <returns> The <see cref="Task"/>. </returns>
        Task RelayMessage(ITurnContext context, Activity activity);
    }
}