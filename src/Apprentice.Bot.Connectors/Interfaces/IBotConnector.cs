namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Dto
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.Bot.Schema;

    public interface IBotConnector
    {
        Task<StartConversationResponse> StartConversationAsync();

        Task<PostToBotResponse> PostToBotAsync(string conversationId, BotMessage message);

        Task<IEnumerable<Activity>> GetMessages();

        Task EndConversation();
    }
}