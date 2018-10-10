namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Dto;

    using Microsoft.Bot.Schema;

    public interface IBotConnector
    {
        Task<StartConversationResponse> StartConversationAsync();

        Task<PostToBotResponse> PostToBotAsync(string conversationId, BotMessage message);

        Task<IEnumerable<Activity>> GetMessages();

        Task EndConversation();
    }
}