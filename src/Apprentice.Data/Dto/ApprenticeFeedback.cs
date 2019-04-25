namespace ESFA.DAS.ProvideFeedback.Apprentice.Data.Dto
{
    using System;
    using System.Collections.Generic;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Feedback;

    using Newtonsoft.Json;

    [Serializable]
    public class ApprenticeFeedback : TypedDocument<ApprenticeFeedback>
    {
        [JsonProperty("surveyId")]
        public string SurveyId { get; set; }

        [JsonProperty("startTime")]
        public DateTime StartTime { get; set; }

        [JsonProperty("finishTime")]
        public DateTime FinishTime { get; set; }

        [JsonProperty("apprentice")]
        public Apprentice Apprentice { get; set; }

        [JsonProperty("apprenticeship")]
        public Apprenticeship Apprenticeship { get; set; }

        [JsonProperty("responses")]
        public List<ApprenticeResponse> Responses { get; set; }
    }
}
