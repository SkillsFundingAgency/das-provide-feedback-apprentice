namespace ESFA.DAS.ProvideFeedback.Apprentice.Data.Repositories
{
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Data.Dto;

    public interface IConversationRepository
    {
        Task SaveConversation(ConversationLog conversationLog);
    }

    public class CosmosConversationRepository : CosmosDbRepositoryBase<CosmosConversationRepository>, IConversationRepository
    {
        public async Task SaveConversation(ConversationLog conversationLog)
        {
            await this.UpsertItemAsync(conversationLog);
        }
    }
}