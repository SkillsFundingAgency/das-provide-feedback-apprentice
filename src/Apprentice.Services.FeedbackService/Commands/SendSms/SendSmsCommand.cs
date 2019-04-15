using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Dto;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.Interfaces;
using Microsoft.Azure.ServiceBus;
using System;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Services.FeedbackService.Commands.SendSms
{
    public class SendSmsCommand : ICommand
    {
        public OutgoingSms Message { get; set; }
        public Message QueueMessage { get; set; }        

        public SendSmsCommand(OutgoingSms message, Message queueMessage)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
            QueueMessage = queueMessage ?? throw new ArgumentNullException(nameof(queueMessage));
        }
    }
}
