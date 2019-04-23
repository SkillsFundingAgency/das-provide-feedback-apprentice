using ESFA.DAS.ProvideFeedback.Apprentice.Core.Exceptions;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.Interfaces;
using ESFA.DAS.ProvideFeedback.Apprentice.Data.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Services.FeedbackService.Commands.SendSms
{
    public class SendSmsCommandHandlerWithOrderCheck : ICommandHandlerAsync<SendSmsCommand>
    {
        private readonly ICommandHandlerAsync<SendSmsCommand> _handler;
        private readonly IConversationRepository _conversationRepository;
        public SendSmsCommandHandlerWithOrderCheck(ICommandHandlerAsync<SendSmsCommand> handler, IConversationRepository conversationRepository)
        {
            _handler = handler;
            _conversationRepository = conversationRepository;
        }

        public void Handle(SendSmsCommand command)
        {
            _handler.Handle(command);
        }

        public async Task HandleAsync(SendSmsCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            var conversation = command.Message.Conversation.ToConversation();

            var lastConversation = await _conversationRepository.Get(conversation.Id);

            if (lastConversation != null && command.Message.Conversation.TurnId != lastConversation.TurnId + 1)
            {
                if(command.Message.Conversation.TurnId <= lastConversation.TurnId)
                {
                    return; // don't process messages already sent
                }
                throw new OutOfOrderException($"Message for conversation {conversation.Id} processed out of order.  Expected turnId {lastConversation.TurnId + 1} but received turnId {command.Message.Conversation.TurnId} with activityId {command.Message.Conversation.ActivityId}");
            }

            await _handler.HandleAsync(command, cancellationToken);
        }
    }
}
