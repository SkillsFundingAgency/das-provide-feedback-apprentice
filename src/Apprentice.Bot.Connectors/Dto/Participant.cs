namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Dto
{
    using System;

    using Newtonsoft.Json;

    [Serializable]
    public class Participant
    {
        [JsonProperty("id")]
        public string PhoneNumber { get; set; }
    }
}