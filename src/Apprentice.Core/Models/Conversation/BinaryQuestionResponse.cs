using System;
using Newtonsoft.Json;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Core.Models
{
    [Serializable]
    public class BinaryQuestionResponse
    {
        [JsonProperty("answer")]
        public string Answer { get; set; }

        [JsonProperty("question")]
        public string Question { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }

        [JsonProperty("intent")]
        public string Intent { get; set; }

        [JsonProperty("isPositive")]
        public bool IsPositive => this.Score > 0;
    }
}