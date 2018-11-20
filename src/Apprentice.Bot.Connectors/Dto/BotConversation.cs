namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Dto
{
    using System;

    using Newtonsoft.Json;

    [Serializable]
    public class BotConversation
    {
        [JsonProperty("id")]
        public string ConversationId { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("turnId")]
        public int TurnId { get; set; }
    }
}