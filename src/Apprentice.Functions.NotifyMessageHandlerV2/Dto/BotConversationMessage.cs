namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.Dto
{
    using System;

    using Newtonsoft.Json;

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