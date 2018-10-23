using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2
{
    public static class DailySurveyTrigger
    {
        private static IApprenticeDetailRepository _apprenticeDetailRepository = new InMemoryApprenticeDetailRepository();

        [FunctionName("DailySurveyTrigger")]
        public static void Run([TimerTrigger("0 0 11 * * MON-FRI")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Daily survey trigger started");

            var batchSize = 100;
            var apprenticeDetails = _apprenticeDetailRepository.GetApprenticeDetails(batchSize);

            foreach (var apprenticeDetail in apprenticeDetails)
            {
                InitiateSurvey(apprenticeDetail);
            }
        }

        private static void InitiateSurvey(ApprenticeDetail apprenticeDetail)
        {
            throw new NotImplementedException();
        }
    }
}
