namespace ESFA.DAS.ProvideFeedback.Apprentice.Core.State
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Conversation;

    [Serializable]
    public class SurveyState
    {
        public SurveyState()
        {
        }

        public List<IQuestionResponse> Responses { get; set; } = new List<IQuestionResponse>();

        public string SurveyId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public ProgressState Progress { get; set; }

        public int Score => this.Responses.Sum(pqr => pqr?.Score ?? 0);
    }
}