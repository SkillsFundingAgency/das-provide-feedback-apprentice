using System;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Data.Dto
{
    public class Conversation
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string ActivityId { get; set; }

        public long TurnId { get; set; }
    }
}
