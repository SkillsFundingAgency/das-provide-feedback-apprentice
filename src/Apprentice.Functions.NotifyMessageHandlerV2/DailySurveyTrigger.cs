using System;
using System.Collections.Generic;
using System.Dynamic;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Conversation;
using ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2
{
    public static class DailySurveyTrigger
    {
        private static IApprenticeDetailRepository _apprenticeDetailRepository = new InMemoryApprenticeDetailRepository();

        [FunctionName("DailySurveyTrigger")]
        public static void Run(
            [TimerTrigger("0 0 11 * * MON-FRI")]TimerInfo myTimer,
            ILogger log,
            [ServiceBus("sms-incoming-messages", Connection = "ServiceBusConnection", EntityType = Microsoft.Azure.WebJobs.ServiceBus.EntityType.Queue)] out string msg)
        {
            msg = string.Empty;
            log.LogInformation($"Daily survey trigger started");

            var batchSize = 100;
            var apprenticeDetails = _apprenticeDetailRepository.GetApprenticeDetails(batchSize);

            foreach (var apprenticeDetail in apprenticeDetails)
            {
                var trigger = new SmsConversationTrigger()
                {
                    Id = Guid.NewGuid().ToString(),
                    SourceNumber = apprenticeDetail.PhoneNumber,
                    DestinationNumber = null,
                    Message = $"start {apprenticeDetail.UniqueSurveyCode}",
                    TimeStamp = DateTime.UtcNow
                };

                var payload = new KeyValuePair<string, SmsConversationTrigger>("bot-manual-trigger", trigger);

                msg = JsonConvert.SerializeObject(payload);
            }
        }
    }
}
