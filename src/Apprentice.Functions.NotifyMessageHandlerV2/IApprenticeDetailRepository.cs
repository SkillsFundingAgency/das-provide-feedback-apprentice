using System.Collections.Generic;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2
{
    internal interface IApprenticeDetailRepository
    {
        IEnumerable<ApprenticeDetail> GetApprenticeDetails(int batchSize);
    }
}