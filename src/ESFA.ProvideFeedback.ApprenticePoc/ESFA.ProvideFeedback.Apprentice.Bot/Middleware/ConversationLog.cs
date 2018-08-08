using Newtonsoft.Json;

namespace ESFA.ProvideFeedback.Apprentice.Bot.Middleware
{
    public class ConversationLog
    {
        [JsonProperty("from")]
        public dynamic From;

        [JsonProperty("recipient")]
        public dynamic Recipient;

        [JsonProperty("conversation")]
        public dynamic Conversation;

        [JsonProperty("channelData")]
        public dynamic ChannelData;

        [JsonProperty("channelId")]
        public string ChannelId;

        [JsonProperty("time")]
        public string Time;

        [JsonProperty("messageReceived")]
        public string Message;

        [JsonProperty("replySent")]
        public string Reply;
    }
}