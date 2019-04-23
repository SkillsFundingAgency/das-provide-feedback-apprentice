using System;
using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.Interfaces;
using ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.DependecyInjection;
using ESFA.DAS.ProvideFeedback.Apprentice.Services.FeedbackService.Commands.TriggerSurveyInvites;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

public static class DailySurveyTrigger
{
    [FunctionName("DailySurveyTrigger")]
    public static async Task Run(
        [TimerTrigger("%DailySurveyTriggerSchedule%")]TimerInfo myTimer,
        [Inject]ICommandHandlerAsync<TriggerSurveyInvitesCommand> commandHandler,
        ILogger log,
        ExecutionContext executionContext)
    {
        try
        {
            log.LogInformation("Daily survey trigger started");
            await commandHandler.HandleAsync(new TriggerSurveyInvitesCommand());
            log.LogInformation("Daily survey trigger complete");
        }
        catch (Exception ex)
        {
            log.LogError(ex, ex.Message);
            throw ex;
        }
    }
}
