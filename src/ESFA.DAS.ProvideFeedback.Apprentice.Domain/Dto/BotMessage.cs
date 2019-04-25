namespace ESFA.DAS.ProvideFeedback.Apprentice.Domain.Dto
{
    using System;

    using Newtonsoft.Json;

    [Serializable]
    public class BotMessage
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