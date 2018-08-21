namespace ESFA.ProvideFeedback.Apprentice.NotifyMessageHandler.Dto
{
    using System;

    using Newtonsoft.Json;

    [Serializable]
    public class NotifyMessage
    {
        [JsonProperty("date_received")]
        public string DateReceived { get; set; }

        [JsonProperty("destination_number")]
        public string DestinationNumber { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("source_number")]
        public string SourceNumber { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}