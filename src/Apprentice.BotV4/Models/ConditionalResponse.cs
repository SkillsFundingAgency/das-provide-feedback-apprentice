namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.State;

    public interface IResponse
    {
        string Id { get; set; }

        string Prompt { get; set; }
    }

    public abstract class ConditionalResponse<T> : IResponse
    {
        public string Id { get; set; }

        public string Prompt { get; set; }

        public abstract bool IsValid(T userResponse);
    }

    public class PositiveResponse : ConditionalResponse<BinaryQuestionResponse>
    {
        public override bool IsValid(BinaryQuestionResponse userResponse)
        {
            return userResponse.IsPositive;
        }
    }

    public class NegativeResponse : ConditionalResponse<BinaryQuestionResponse>
    {
        public override bool IsValid(BinaryQuestionResponse userResponse)
        {
            return !userResponse.IsPositive;
        }
    }

    public class StaticResponse : IResponse
    {
        public string Id { get; set; }

        public string Prompt { get; set; }
    }

    public class PredicateResponse : ConditionalResponse<UserInfo>
    {
        public Func<UserInfo, bool> Predicate { get; set; }

        public override bool IsValid(UserInfo state)
        {
            return this.Predicate.Invoke(state);
        }
    }
}