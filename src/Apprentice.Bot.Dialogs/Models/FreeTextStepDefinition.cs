namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Models
{
    using System;

    using Newtonsoft.Json;

    [Serializable]
    public class FreeTextStepDefinition : SurveyStepDefinitionBase
    {
        [JsonProperty("prompt")]
        public string Prompt { get; set; }
    }
}