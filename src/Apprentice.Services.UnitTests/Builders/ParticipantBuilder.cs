using System;
using ESFA.DAS.ProvideFeedback.Apprentice.Domain.Dto;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Services.UnitTests.Builders
{
    public class ParticipantBuilder
    {
        private string _userId = Guid.NewGuid().ToString();

        public Participant Build()
        {
            return new Participant
            {
                UserId = _userId
            };
        }
        public ParticipantBuilder WithUserId(string userId)
        {
            _userId = userId;
            return this;
        }

        public static implicit operator Participant(ParticipantBuilder instance)
        {
            return instance.Build();
        }
    }    
}
