using System.Threading.Tasks;
using ApprenticeFeedbackDto = ESFA.DAS.ProvideFeedback.Apprentice.Data.Dto.ApprenticeFeedback;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Data.Repositories
{
    public interface IFeedbackRepository
    {
        Task SaveFeedback(ApprenticeFeedbackDto feedback);
    }

    public class CosmosFeedbackRepository : CosmosDbRepositoryBase<CosmosFeedbackRepository>, IFeedbackRepository
    {
        public Task SaveFeedback(ApprenticeFeedbackDto feedback)
        {
            return this.UpsertItemAsync(feedback);
        }
    }
}
