using ESFA.DAS.ProvideFeedback.Apprentice.Core.Exceptions;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Services.FeedbackService.Commands.SendSms
{
    public class SendSmsCommandHandlerWithLocking : ICommandHandlerAsync<SendSmsCommand>
    {
        private readonly ICommandHandlerAsync<SendSmsCommand> _handler;
        private readonly IDistributedLockProvider _lockProvider;
        public SendSmsCommandHandlerWithLocking(ICommandHandlerAsync<SendSmsCommand> handler, IDistributedLockProvider lockProvider)
        {
            _handler = handler;
            _lockProvider = lockProvider;
        }

        public void Handle(SendSmsCommand command)
        {
            _handler.Handle(command);
        }

        public async Task HandleAsync(SendSmsCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _lockProvider.Start();

            try
            {
                var conversation = command.Message.Conversation.ToConversation();

                var lockId = conversation.Id.ToString();

                try
                {
                    if (!await _lockProvider.AcquireLock(lockId, cancellationToken))
                    {
                        throw new ConversationLockedException($"A message for conversation {lockId} is already being processed.");
                    }

                    await _handler.HandleAsync(command);
                }
                finally
                {
                    await _lockProvider.ReleaseLock(lockId);
                }
            }
            finally
            {
                await _lockProvider.Stop();
            }       
        }
    }
}
