using System;
using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Dto;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.Interfaces;
using ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.Application.Commands;
using ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.DependecyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

public static class DailySurveyTrigger
{
    [FunctionName("DailySurveyTrigger")]
    public static async Task Run(
        [TimerTrigger("%DailySurveyTriggerSchedule%", RunOnStartup = true)]TimerInfo myTimer,
        [ServiceBus("sms-incoming-messages", Connection = "ServiceBusConnection", EntityType = Microsoft.Azure.WebJobs.ServiceBus.EntityType.Queue)]
        IAsyncCollector<IncomingSms> outputSbQueue,
        [Inject]ICommandHandlerAsync<TriggerSurveyInvitesCommand> commandHandler,
        ILogger log,
        ExecutionContext executionContext)
    {
        try
        {
            log.LogInformation("Daily survey trigger started");
            await commandHandler.HandleAsync(new TriggerSurveyInvitesCommand(outputSbQueue));
            log.LogInformation("Daily survey trigger complete");
        }
        catch (Exception ex)
        {
            log.LogError(ex, ex.Message);
            throw ex;
        }
    }
}
