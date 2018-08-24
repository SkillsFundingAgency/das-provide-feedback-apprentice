namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Dialogs;

    public class ApprenticeFeedback
    {
        public ApprenticeFeedback()
        {
            this.Responses = new List<PolarQuestionResponse>();
        }

        public List<PolarQuestionResponse> Responses { get; set; }

        public string SurveyId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public ProgressState Progress { get; set; }

        public int Score => this.Responses.Sum(r => r.Score);
    }
}