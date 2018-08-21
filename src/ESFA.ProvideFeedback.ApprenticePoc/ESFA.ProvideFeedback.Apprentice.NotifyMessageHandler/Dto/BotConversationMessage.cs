namespace ESFA.ProvideFeedback.Apprentice.NotifyMessageHandler.Dto
{
    using System;

    using ESFA.ProvideFeedback.Apprentice.Data;

    using Newtonsoft.Json;

    [Serializable]
    public class BotConversation : TypedDocument<BotConversation>
    {
        [JsonProperty("conversation_id")]
        public string ConversationId { get; set; }

        [JsonProperty("mobile_number")]
        public string MobileNumber { get; set; }
    }

    [Serializable]
    public class BotConversationMessage
    {
        [JsonProperty("channelData")]
        public dynamic ChannelData { get; set; }

        [JsonProperty("from")]
        public dynamic From { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}