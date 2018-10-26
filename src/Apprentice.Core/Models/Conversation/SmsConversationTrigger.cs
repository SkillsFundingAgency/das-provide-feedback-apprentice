
namespace ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Conversation
{
    using System;

    using Newtonsoft.Json;

    public class SmsConversationTrigger
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("sourceNumber")]
        public string SourceNumber { get; set; }

        [JsonProperty("destinationNumber")]
        public string DestinationNumber { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("dateReceived")]
        public DateTime DateReceived { get; set; }

        [JsonProperty("uniqueLearnerNumber")]
        public string UniqueLearnerNumber { get; set; }
    }
}