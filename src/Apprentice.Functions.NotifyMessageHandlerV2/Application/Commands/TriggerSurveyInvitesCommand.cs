using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Dto;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.Interfaces;
using Microsoft.Azure.WebJobs;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.Application.Commands
{
    public class TriggerSurveyInvitesCommand : ICommand
    {
        public IAsyncCollector<IncomingSms> OutputSbQueue { get; private set; }
        public TriggerSurveyInvitesCommand(IAsyncCollector<IncomingSms> outputSbQueue)
        {
            OutputSbQueue = outputSbQueue;
        }
    }
}
