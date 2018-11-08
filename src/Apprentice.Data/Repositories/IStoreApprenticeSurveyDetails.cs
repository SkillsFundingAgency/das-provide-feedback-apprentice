using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Apprentice.Data.Dto;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Data.Repositories
{
    public interface IStoreApprenticeSurveyDetails
    {
        Task<IEnumerable<ApprenticeSurveyDetail>> GetApprenticeSurveyDetailsAsync(int batchSize);

        Task SetApprenticeSurveySentAsync(long mobileNumber, string surveyCode);
    }
}
