using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;

namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models
{
    using System;

    public class PredicateResponse : ConditionalResponse<UserInfo>
    {
        public Func<UserInfo, bool> Predicate { get; set; }

        public override bool IsValid(UserInfo state)
        {
            return this.Predicate.Invoke(state);
        }
    }
}