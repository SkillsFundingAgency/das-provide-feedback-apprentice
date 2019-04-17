namespace ESFA.DAS.ProvideFeedback.Apprentice.Domain.Dto
{
    using System;

    using Newtonsoft.Json;

    [Serializable]
    public class PostToBotResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonIgnore]
        public string ConversationId => this.Id.Split(Convert.ToChar("|"))[0] ?? throw new Exception("Could not parse conversationId");

        [JsonIgnore]
        public int Turn => Convert.ToInt32(this.Id.Split(Convert.ToChar("|"))[1] ?? throw new Exception("Could not parse turnId"));
    }
}