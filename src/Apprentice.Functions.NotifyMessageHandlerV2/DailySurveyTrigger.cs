using System;
using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Dto;
using ESFA.DAS.ProvideFeedback.Apprentice.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.DependecyInjection;
using ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

public static class DailySurveyTrigger
{
    private static IStoreApprenticeSurveyDetails _surveyDetailsRepo;

    [FunctionName("DailySurveyTrigger")]
    public static async Task Run(
        [TimerTrigger("%DailySurveyTriggerSchedule%")]TimerInfo myTimer,
        [Inject]IStoreApprenticeSurveyDetails surveyDetailsRepo,
        ILogger log,
        [ServiceBus("sms-incoming-messages", Connection = "ServiceBusConnection", EntityType = Microsoft.Azure.WebJobs.ServiceBus.EntityType.Queue)]
        IAsyncCollector<IncomingSms> outputSbQueue,
        [Inject] SettingsProvider settingsProvider,
        ExecutionContext executionContext)
    {
        try
        {
            _surveyDetailsRepo = surveyDetailsRepo;
            log.LogInformation($"Daily survey trigger started");

            var batchSize = settingsProvider.GetInt("ApprenticeBatchSize");
            var apprenticeDetails = await _surveyDetailsRepo.GetApprenticeSurveyInvitesAsync(batchSize);

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

                // TODO: try catch here to allow subsequent messages to be sent if one fails
                // TODO: implement some sort of transaction here

                //Poly used within here in case of transient failure.
                await _surveyDetailsRepo.SetApprenticeSurveySentAsync(apprenticeDetail.UniqueLearnerNumber, apprenticeDetail.SurveyCode);

                // TODO: investigate any retry policy AddAsync implements
                await outputSbQueue.AddAsync(trigger);
                await Task.Delay(250);
            }            
        }
        catch(Exception ex)
        {
            log.LogError(ex, ex.Message);
            throw ex;
        }
    }
}
