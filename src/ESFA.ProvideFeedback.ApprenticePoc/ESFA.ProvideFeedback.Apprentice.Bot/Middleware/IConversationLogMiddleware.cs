// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IConversationLogMiddleware.cs" company="Education & Skills Funding Agency">
//   Copyright © 2018 Education & Skills Funding Agency
// </copyright>
// <summary>
//   Defines the IConversationLogMiddleware contracts.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace ESFA.ProvideFeedback.Apprentice.Bot.Middleware
{
    using System.Threading.Tasks;

    using Microsoft.Bot.Builder;

    /// <inheritdoc />
    /// <summary>
    /// Logs the conversation to a persistent data store
    /// </summary>
    public interface IConversationLogMiddleware : IMiddleware
    {
        /// <summary>
        /// Create a new conversation log and store it to the data store
        /// </summary>
        /// <param name="context"> The context. </param>
        /// <param name="botReply"> The bot reply. </param>
        /// <returns> The <see cref="Task"/>. </returns>
        Task CreateConversationLog(ITurnContext context, string botReply);
    }
}