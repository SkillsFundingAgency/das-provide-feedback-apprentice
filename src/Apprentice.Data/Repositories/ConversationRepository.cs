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
        public Task SaveConversation(ConversationLog conversationLog)
        {
            return this.UpsertItemAsync(conversationLog);
        }
    }
}