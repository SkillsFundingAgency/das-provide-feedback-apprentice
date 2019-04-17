namespace ESFA.DAS.ProvideFeedback.Apprentice.Domain.Dto
{
    using System;

    using Newtonsoft.Json;

    [Serializable]
    public class Participant
    {
        [JsonProperty("id")]
        public string UserId { get; set; }
    }
}