namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Dialogs;

    using Newtonsoft.Json;

    [Serializable]
    public class SurveyState
    {
        public SurveyState()
        {
            this.Responses = new List<BinaryQuestionResponse>();
        }

        [JsonProperty("responses")]
        public List<BinaryQuestionResponse> Responses { get; set; }

        [JsonProperty("surveyId")]
        public string SurveyId { get; set; }

        [JsonProperty("startDate")]
        public DateTime StartDate { get; set; }

        [JsonProperty("endDate")]
        public DateTime EndDate { get; set; }

        [JsonProperty("progress")]
        public ProgressState Progress { get; set; }

        [JsonProperty("score")]
        public int Score => this.Responses.Select(r => r as BinaryQuestionResponse).Sum(pqr => pqr?.Score ?? 0);
    }
}