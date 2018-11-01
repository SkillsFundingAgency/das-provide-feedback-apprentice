namespace ESFA.DAS.ProvideFeedback.Apprentice.Data.Repositories
{
    using System.Threading.Tasks;
    using Microsoft.Extensions.Options;

    using ApprenticeFeedbackDto = ESFA.DAS.ProvideFeedback.Apprentice.Data.Dto.ApprenticeFeedback;

    public interface IFeedbackRepository
    {
        Task SaveFeedback(ApprenticeFeedbackDto feedback);
    }

    public class CosmosFeedbackRepository : CosmosDbRepositoryBase<CosmosFeedbackRepository>, IFeedbackRepository
    {
        public async Task SaveFeedback(ApprenticeFeedbackDto feedback)
        {
            await this.UpsertItemAsync(feedback);
        }
    }
}
