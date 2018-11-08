namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Models
{
    using System;

    using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;

    public class PredicateBotResponse : ConditionalBotResponse
    {
        public Func<SurveyState, bool> Predicate { get; set; }

        public override bool IsValid(SurveyState state)
        {
            return this.Predicate.Invoke(state);
        }
    }
}