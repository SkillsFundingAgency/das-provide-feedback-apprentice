namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models
{
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public abstract class SurveyStepBase : ISurveyStep
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("isValid")]
        public bool IsValid { get; set; }

        [JsonProperty("order")]
        public int Order { get; set; }

        [JsonProperty("responses")]
        public ICollection<IResponse> Responses { get; set; } = new List<IResponse>();
    }
}