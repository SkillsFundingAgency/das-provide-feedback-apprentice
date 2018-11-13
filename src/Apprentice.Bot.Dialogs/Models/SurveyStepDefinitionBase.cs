namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Models
{
    using System.Collections.Generic;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Interfaces;

    using Newtonsoft.Json;

    public abstract class SurveyStepDefinitionBase : ISurveyStepDefinition
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("isValid")]
        public bool IsValid { get; set; } = true; // TODO: Add survey builder UI

        [JsonProperty("order")]
        public int Order { get; set; } // TODO: Add survey builder UI

        [JsonProperty("responses")]
        public ICollection<IBotResponse> Responses { get; set; } = new List<IBotResponse>();
    }
}