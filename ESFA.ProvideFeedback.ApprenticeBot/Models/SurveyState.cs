using System.Collections.Generic;

namespace ESFA.ProvideFeedback.ApprenticeBot
{
    /// <summary>
    /// Stores the current conversation state
    /// </summary>
    public class SurveyState: Dictionary<string, object>
    {
        private const string SurveyScoreKey = "SurveyScore";

        public SurveyState()
        {
            this[SurveyScoreKey] = 0;
        }
        public int SurveyScore
        {
            get { return (int)this[SurveyScoreKey]; }
            set { this[SurveyScoreKey] = value; }
        }

    }
}