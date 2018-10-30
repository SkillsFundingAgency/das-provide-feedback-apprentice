
namespace ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Conversation
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
        public DateTime DateReceived { get; set; }

        [JsonProperty("unique_learner_number")]
        public string UniqueLearnerNumber { get; set; }
    }
}