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

         [JsonProperty("activityId")]
        public string ActivityId { get; set; }

        [JsonProperty("turnId")]
        public long TurnId { get; set; }
    }
}