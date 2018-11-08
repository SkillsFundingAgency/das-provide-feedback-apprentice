namespace ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Conversation
{
    using System;

    using Newtonsoft.Json;

    [Serializable]
    public class QuestionResponse : IQuestionResponse
    {
        [JsonProperty("answer")]
        public string Answer { get; set; }

        [JsonProperty("question")]
        public string Question { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; } = 0;

        [JsonProperty("intent")]
        public string Intent { get; set; }

        [JsonProperty("isPositive")]
        public bool IsPositive => this.Intent == "yes";
    }
}