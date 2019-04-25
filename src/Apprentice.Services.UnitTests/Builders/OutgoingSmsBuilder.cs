using System;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Conversation;
using ESFA.DAS.ProvideFeedback.Apprentice.Domain.Dto;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Services.UnitTests.Builders
{
    public class OutgoingSmsBuilder
    {
        private string _messageText = Guid.NewGuid().ToString();
        private Participant _participant = new ParticipantBuilder();
        private BotConversation _conversation = new BotConversationBuilder();

        public OutgoingSms Build()
        {
            return new OutgoingSms
            {
                From = _participant,
                Message = _messageText,
                Conversation = _conversation
            };
        }
        public OutgoingSmsBuilder WithParticipant(Participant participant)
        {
            _participant = participant;
            return this;
        }
        public OutgoingSmsBuilder WithMessageText(string messageText)
        {
            _messageText = messageText;
            return this;
        }
        public OutgoingSmsBuilder WithConversation(BotConversation conversation)
        {
            _conversation = conversation;
            return this;
        }
        public static implicit operator OutgoingSms(OutgoingSmsBuilder instance)
        {
            return instance.Build();
        }
    }
}
