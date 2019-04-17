
using ESFA.DAS.ProvideFeedback.Apprentice.Data.Dto;
using System;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Services.UnitTests.Builders
{
    public class ConversationBuilder
    {
        private string _id = Guid.NewGuid().ToString();
        private Guid _userId = Guid.NewGuid();
        private string _activityId = Guid.NewGuid().ToString();
        private long _turnId = 1;

        public Conversation Build()
        {
            return new Conversation
            {
                Id = _id,
                ActivityId = _activityId,
                TurnId = _turnId
            };
        }
        public ConversationBuilder WithId(string id)
        {
            _id = id;
            return this;
        }
        public ConversationBuilder WithUserId(Guid userId)
        {
            _userId = userId;
            return this;
        }
        public ConversationBuilder WithActivityId(string activityId)
        {
            _activityId = activityId;
            return this;
        }
        public ConversationBuilder WithTurnId(long turnId)
        {
            _turnId = turnId;
            return this;
        }

        public static implicit operator Conversation(ConversationBuilder instance)
        {
            return instance.Build();
        }
    }
}
