using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Conversation;
using System;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Services.UnitTests.Builders
{
    public class BotConversationBuilder
    {
        private string _conversationId = Guid.NewGuid().ToString();
        private string _userId = Guid.NewGuid().ToString();
        private string _activityId = Guid.NewGuid().ToString();
        private long _turnId = 1;

        public BotConversation Build()
        {
            return new BotConversation
            {
                ConversationId = _conversationId,
                UserId = _userId,
                ActivityId = _activityId,
                TurnId = _turnId
            };
        }
        public BotConversationBuilder WithConversationId(string conversationId)
        {
            _conversationId = conversationId;
            return this;
        }
        public BotConversationBuilder WithUserId(string userId)
        {
            _userId = userId;
            return this;
        }
        public BotConversationBuilder WithActivityId(string activityId)
        {
            _activityId = activityId;
            return this;
        }
        public BotConversationBuilder WithTurnId(long turnId)
        {
            _turnId = turnId;
            return this;
        }

        public static implicit operator BotConversation(BotConversationBuilder instance)
        {
            return instance.Build();
        }
    }
}
