namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.Dto
{
    using System;

    using ESFA.DAS.ProvideFeedback.Apprentice.Data;

    using Newtonsoft.Json;

    [Serializable]
    public class BotConversation : TypedDocument<BotConversation>
    {
        [JsonProperty("conversationId")]
        public string ConversationId { get; set; }

        [JsonProperty("mobileNumber")]
        public string MobileNumber { get; set; }

        [JsonProperty("turnId")]
        public int TurnId { get; set; }

        [JsonProperty("uniqueLearnerNumber")]
        public string UniqueLearnerNumber { get; set; }
    }
}