
namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Commands.Dialog
{
    using System;

    using Newtonsoft.Json;

    public class SmsConversationTrigger
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("source_number")]
        public string SourceNumber { get; set; }

        [JsonProperty("destination_number")]
        public string DestinationNumber { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("date_received")]
        public DateTime TimeStamp { get; set; }
    }
}