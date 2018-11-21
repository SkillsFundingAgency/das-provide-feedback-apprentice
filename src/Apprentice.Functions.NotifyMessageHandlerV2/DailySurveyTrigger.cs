using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Dto;
using ESFA.DAS.ProvideFeedback.Apprentice.Data.Dto;
using ESFA.DAS.ProvideFeedback.Apprentice.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.DependecyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

public static class DailySurveyTrigger
{
    private static IStoreApprenticeSurveyDetails _surveyDetailsRepo;

    [FunctionName("DailySurveyTrigger")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req,
        [Inject]IStoreApprenticeSurveyDetails surveyDetailsRepo,
        ILogger log,
        [ServiceBus("sms-incoming-messages", Connection = "ServiceBusConnection", EntityType = Microsoft.Azure.WebJobs.ServiceBus.EntityType.Queue)]
        IAsyncCollector<IncomingSms> outputSbQueue,
        ExecutionContext executionContext)
    {
        try
        {
            _surveyDetailsRepo = surveyDetailsRepo;
            log.LogInformation($"Daily survey trigger started");

            var batchSize = 200;
            var apprenticeDetails = await GetApprenticeDetailsToSendSurvey(batchSize);

            foreach (var apprenticeDetail in apprenticeDetails)
            {
                var now = DateTime.Now;
                var trigger = new IncomingSms()
                {
                    Type = SmsType.SurveyInvitation,
                    Id = Guid.NewGuid().ToString(),
                    SourceNumber = apprenticeDetail.MobileNumber.ToString(),
                    DestinationNumber = null,
                    Message = $"bot_dialog_start {apprenticeDetail.SurveyCode}",
                    DateReceived = now,
                    UniqueLearnerNumber = apprenticeDetail.UniqueLearnerNumber,
                    StandardCode = apprenticeDetail.StandardCode,
                    ApprenticeshipStartDate = apprenticeDetail.ApprenticeshipStartDate
                };

                await outputSbQueue.AddAsync(trigger);

                await _surveyDetailsRepo.SetApprenticeSurveySentAsync(apprenticeDetail.UniqueLearnerNumber, apprenticeDetail.SurveyCode);
                await Task.Delay(250);
            }            
        }
        catch(Exception ex)
        {
            log.LogError(ex, ex.Message);
            throw ex;
        }

        return new OkResult();
    }

    private static Task<IEnumerable<ApprenticeSurveyInvite>> GetApprenticeDetailsToSendSurvey(int batchSize)
    {
        return _surveyDetailsRepo.GetApprenticeSurveyInvitesAsync(batchSize);
    }
}
