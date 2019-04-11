using ESFA.DAS.ProvideFeedback.Apprentice.Core.Exceptions;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.Interfaces;
using ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.Application.Commands;
using ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.Application.CommandHandlers
{
    public class SendSmsCommandHandlerWithWaitForPreviousSms : ICommandHandlerAsync<SendSmsCommand>
    {
        private readonly ICommandHandlerAsync<SendSmsCommand> _handler;
        private readonly INotificationClient _notificationClient;

        public SendSmsCommandHandlerWithWaitForPreviousSms(
            ICommandHandlerAsync<SendSmsCommand> handler, 
            INotificationClient notificationClient)
        {
            _handler = handler;
            _notificationClient = notificationClient;
        }

        public void Handle(SendSmsCommand command)
        {
            _handler.Handle(command);
        }

        public async Task HandleAsync(SendSmsCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            var reference = command.Message.Conversation.ConversationId;

            bool isSent = await IsSent(command);

            if (!isSent)
            {
                throw new PreviousMessageNotSentException("Previous SMS message for conversation {reference} has not yet been sent.");
            }

            await _handler.HandleAsync(command, cancellationToken);
            return;
        }

        private async Task<bool> IsSent(SendSmsCommand command)
        {
            var notificationReference = command.Message.Conversation.ConversationId;

            var notificationList = await _notificationClient.GetNotifications("sms", string.Empty, notificationReference);

            var lastNotification = notificationList.notifications
                .OrderByDescending(n => n.createdAt)
                .FirstOrDefault();

            return lastNotification == null || lastNotification.status == "delivered";
        }
    }
}
