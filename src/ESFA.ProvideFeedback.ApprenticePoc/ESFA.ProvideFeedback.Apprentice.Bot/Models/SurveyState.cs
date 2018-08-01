using System.Collections.Generic;

namespace ESFA.ProvideFeedback.Apprentice.Bot.Models
{
    /// <inheritdoc />
    /// <summary>
    /// Stores the current conversation state
    /// </summary>
    public class SurveyState: Dictionary<string, object>
    {
        private const string SurveyScoreKey = "SurveyScore";
        private const string MessagesKey = "Messages";

        public SurveyState()
        {
            this[SurveyScoreKey] = 0;
            this[MessagesKey] = 0;
        }
        public int SurveyScore
        {
            get => (int)this[SurveyScoreKey];
            set => this[SurveyScoreKey] = value;
        }

        public List<string> Messages
        {
            get => (List<string>) this[MessagesKey];
            set => this[MessagesKey] = value;
        }
    }
}