namespace ESFA.DAS.ProvideFeedback.Apprentice.Core.Models
{
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    [Serializable]
    public class ApprenticeFeedback
    {
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
