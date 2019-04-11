using ESFA.DAS.ProvideFeedback.Apprentice.Core.Exceptions;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.Interfaces;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Services.FeedbackService.Commands.SendSms
{
    public class SendSmsCommandHandlerWithDelayHandler : ICommandHandlerAsync<SendSmsCommand>
    {
        const string OutOfOrderRetryCount = "OutOfOrderRetryCount";
        const string ConversationLockedRetryCount = "ConversationLockedRetryCount";
        const string PreviousMessageNotSentCount = "PreviousMessageNotSentCount";

        private readonly ICommandHandlerAsync<SendSmsCommand> _handler;
        private readonly IQueueClient _queueClient;
        private readonly ILogger _log;
        private readonly ISettingService _settingService;

        public SendSmsCommandHandlerWithDelayHandler(
            ICommandHandlerAsync<SendSmsCommand> handler, 
            IQueueClient queueClient,
            ILoggerFactory logFactory,
            ISettingService settingService)
        {
            _handler = handler;
            _queueClient = queueClient;
            _log = logFactory.CreateLogger<SendSmsCommandHandlerWithDelayHandler>();
            _settingService = settingService;
        }

        public void Handle(SendSmsCommand command)
        {
            _handler.Handle(command);
        }

        public async Task HandleAsync(SendSmsCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {         
            try
            {
                await _handler.HandleAsync(command);
            }
            catch (ConversationLockedException lockEx)
            {
                await HandleDelay(command.QueueMessage, ConversationLockedRetryCount, () => lockEx);
            }
            catch (OutOfOrderException outOfOrderEx)
            {
                await HandleDelay(command.QueueMessage, OutOfOrderRetryCount, () => outOfOrderEx);
            }
            catch (PreviousMessageNotSentException notSentEx)
            {
                await HandleDelay(command.QueueMessage, PreviousMessageNotSentCount, () => notSentEx);
            }
        }

        private async Task HandleDelay(Message queueMessage, string retryKey, Func<Exception> throwOnExpiry)
        {
            int maxRetryAttempts = _settingService.GetInt("maxRetryAttempts");
            int processingDelay = _settingService.GetInt("retryDelayMs");

            int resubmitCount = queueMessage.UserProperties.ContainsKey(retryKey) ? (int)queueMessage.UserProperties[retryKey] : 0;

            var delayedMessage = queueMessage.Clone();

            if (resubmitCount >= maxRetryAttempts)
            {
                // have delayed long enough so now follow the default processing which will eventually put the message in the dead letter queue.
                throw throwOnExpiry.Invoke();
            }
            else
            {
                ClearExisitingProperties(delayedMessage);
                delayedMessage.ScheduledEnqueueTimeUtc = DateTime.UtcNow.AddMilliseconds(processingDelay);
                delayedMessage.UserProperties[retryKey] = resubmitCount + 1;

                await _queueClient.SendAsync(delayedMessage);
            }

            _log.LogInformation($"{throwOnExpiry.Invoke().Message} Delaying the processing for this message.");

            return;
        }
        private void ClearExisitingProperties(Message queueMessage)
        {
            queueMessage.UserProperties.Remove(OutOfOrderRetryCount);
            queueMessage.UserProperties.Remove(ConversationLockedRetryCount);
            queueMessage.UserProperties.Remove(PreviousMessageNotSentCount);
        }
    }
}
