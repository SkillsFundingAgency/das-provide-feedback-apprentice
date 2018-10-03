namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models
{
    using System.Linq;

    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;

    public class NegativeResponse : ConditionalResponse
    {
        public override bool IsValid(SurveyState state)
        {
            var lastResponse = state.Responses.LastOrDefault();

            return lastResponse != null && !lastResponse.IsPositive;
        }
    }
}