namespace ESFA.DAS.ProvideFeedback.Apprentice.Services
{
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Feedback;

    public interface IFeedbackService
    {
        Task SaveFeedbackAsync(ApprenticeFeedback feedback);

        Task UpdateFeedbackAsync(ApprenticeFeedback feedback);
    }
}