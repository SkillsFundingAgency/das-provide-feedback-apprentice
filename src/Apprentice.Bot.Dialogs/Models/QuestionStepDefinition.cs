namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Models
{
    using System;

    using Newtonsoft.Json;

    [Serializable]
    public class QuestionStepDefinition : SurveyStepDefinitionBase
    {
        [JsonProperty("prompt")]
        public string Prompt { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }
    }
}