namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.Dto
{
    using System;

    using ESFA.DAS.ProvideFeedback.Apprentice.Data;

    using Newtonsoft.Json;

    [Serializable]
    public class BotConversation : TypedDocument<BotConversation>
    {
        [JsonProperty("conversation_id")]
        public string ConversationId { get; set; }

        [JsonProperty("mobile_number")]
        public string MobileNumber { get; set; }
    }
}