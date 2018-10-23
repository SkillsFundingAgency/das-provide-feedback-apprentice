using System.Collections.Generic;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2
{
    internal class InMemoryApprenticeDetailRepository : IApprenticeDetailRepository
    {
        public IEnumerable<ApprenticeDetail> GetApprenticeDetails(int batchSize)
        {
            return new List<ApprenticeDetail>
            {
                new ApprenticeDetail
                {
                    PhoneNumber = "44745995328",
                    Ulr = "7d86f639-4a48-4929-bff1-e36783353b1b",
                    UniqueSurveyCode = "afb-v5",
                    SurveySentDate = null
                }
            };
        }
    }
}