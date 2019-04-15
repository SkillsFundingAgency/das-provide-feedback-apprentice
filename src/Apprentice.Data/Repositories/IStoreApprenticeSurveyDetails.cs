using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Apprentice.Data.Dto;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Data.Repositories
{
    public interface IStoreApprenticeSurveyDetails
    {
        Task<IEnumerable<ApprenticeSurveyInvite>> GetApprenticeSurveyInvitesAsync(int batchSize);

        Task SetApprenticeSurveySentAsync(Guid apprenticeSurveyId);

        Task SetApprenticeSurveyNotSentAsync(Guid apprenticeSurveyId);
    }
}
