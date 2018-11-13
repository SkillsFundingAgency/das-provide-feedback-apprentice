namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Models
{
    using System.Linq;

    using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;

    public class PositiveBotResponse : ConditionalBotResponse
    {
        public override bool IsValid(SurveyState state)
        {
            var lastResponse = state.Responses.LastOrDefault();

            return lastResponse != null && lastResponse.IsPositive;
        }
    }
}