namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.Dto
{
    using System;

    using Newtonsoft.Json;

    [Serializable]
    public class NotifyMessage
    {
        [JsonProperty("dateReceived")]
        public string DateReceived { get; set; }

        [JsonProperty("destinationNumber")]
        public string DestinationNumber { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("sourceNumber")]
        public string SourceNumber { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}