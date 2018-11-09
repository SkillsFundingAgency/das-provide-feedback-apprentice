using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Conversation;
using ESFA.DAS.ProvideFeedback.Apprentice.Data.Dto;
using ESFA.DAS.ProvideFeedback.Apprentice.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.DependecyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public static class DailySurveyTrigger
{
    private static IStoreApprenticeSurveyDetails _surveyDetailsRepo;

    [FunctionName("DailySurveyTrigger")]
    public static async Task Run(
        [TimerTrigger("0 0 11 * * MON-FRI", RunOnStartup = true)]TimerInfo myTimer,
        [Inject]IStoreApprenticeSurveyDetails surveyDetailsRepo,
        ILogger log,
        [ServiceBus("sms-incoming-messages", Connection = "ServiceBusConnection", EntityType = Microsoft.Azure.WebJobs.ServiceBus.EntityType.Queue)]
        ICollector<string> outputSbQueue,
        ExecutionContext executionContext)
    {
        _surveyDetailsRepo = surveyDetailsRepo;
        log.LogInformation($"Daily survey trigger started");

        var batchSize = 100;
        var apprenticeDetails = await GetApprenticeDetailsToSendSurvey(batchSize);

        foreach (var apprenticeDetail in apprenticeDetails)
        {
            var now = DateTime.Now;
            var trigger = new SmsConversationTrigger()
            {
                Id = Guid.NewGuid().ToString(),
                SourceNumber = apprenticeDetail.MobileNumber.ToString(),
                DestinationNumber = null,
                Message = $"start {apprenticeDetail.SurveyCode}",
                DateReceived = now
            };

            var payload = new KeyValuePair<string, SmsConversationTrigger>("bot-manual-trigger", trigger);

            outputSbQueue.Add(JsonConvert.SerializeObject(payload));

            await _surveyDetailsRepo.SetApprenticeSurveySentAsync(apprenticeDetail.MobileNumber, apprenticeDetail.SurveyCode);
        }
    }

    private static Task<IEnumerable<ApprenticeSurveyInvite>> GetApprenticeDetailsToSendSurvey(int batchSize)
    {
        return _surveyDetailsRepo.GetApprenticeSurveyInvitesAsync(batchSize);
    }
}
