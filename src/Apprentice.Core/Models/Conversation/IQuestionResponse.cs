namespace ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Conversation
{
    using Newtonsoft.Json;

    public interface IQuestionResponse
    {
        [JsonProperty("answer")]
        string Answer { get; set; }

        [JsonProperty("question")]
        string Question { get; set; }

        [JsonProperty("score")]
        int Score { get; set; }

        [JsonProperty("intent")]
        string Intent { get; set; }

        [JsonProperty("isPositive")]
        bool IsPositive { get; }
    };
}