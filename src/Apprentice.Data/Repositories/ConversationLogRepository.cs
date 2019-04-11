namespace ESFA.DAS.ProvideFeedback.Apprentice.Data.Repositories
{
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Data.Dto;

    public interface IConversationLogRepository
    {
        Task Save(ConversationLog conversationLog);
    }

    public class CosmosConversationRepository : CosmosDbRepositoryBase<CosmosConversationRepository>, IConversationLogRepository
    {
        public Task Save(ConversationLog conversationLog)
        {
            return this.UpsertItemAsync(conversationLog);
        }
    }
}