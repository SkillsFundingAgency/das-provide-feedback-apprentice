namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models
{
    using System;

    using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;

    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;

    public class PredicateResponse : ConditionalResponse
    {
        public Func<SurveyState, bool> Predicate { get; set; }

        public override bool IsValid(SurveyState context)
        {
            return this.Predicate.Invoke(context);
        }
    }
}