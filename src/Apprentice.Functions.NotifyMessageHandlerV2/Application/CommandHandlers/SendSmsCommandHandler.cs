using ESFA.DAS.ProvideFeedback.Apprentice.Core.Interfaces;
using ESFA.DAS.ProvideFeedback.Apprentice.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.Application.Commands;
using ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.Services;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.Application.CommandHandlers
{
    public class SendSmsCommandHandler : ICommandHandlerAsync<SendSmsCommand>
    {
        private readonly IConversationRepository _conversationRepository;
        private readonly INotificationClient _notificationClient;
        private readonly ISettingService _settingService;
        public SendSmsCommandHandler(
            IConversationRepository conversationRepository, 
            INotificationClient notificationClient, 
            ISettingService settingService)
        {
            _conversationRepository = conversationRepository;
            _notificationClient = notificationClient;
            _settingService = settingService;
        }

        public void Handle(SendSmsCommand command)
        {
            throw new System.NotImplementedException();
        }

        public async Task HandleAsync(SendSmsCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            var conversation = command.Message.Conversation.ToConversation();

            var mobileNumber = command.Message.From.UserId; // TODO: [security] read mobile number from userId hash
            var templateId = _settingService.Get("NotifyTemplateId");
            var personalization = new Dictionary<string, dynamic> { { "message", command.Message.Message } };
            var reference = command.Message.Conversation.ConversationId;
            var smsSenderId = _settingService.Get("NotifySmsSenderId");

            await _notificationClient.SendSms(
                mobileNumber,
                    templateId,
                    personalization,
                    reference,
                    smsSenderId);

            await _conversationRepository.Save(conversation);
        }
    }
}
