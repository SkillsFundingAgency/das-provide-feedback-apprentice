namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Models
{
    using System;

    using Newtonsoft.Json;

    [Serializable]
    public class FreeTextStep : SurveyStepBase
    {
        [JsonProperty("prompt")]
        public string Prompt { get; set; }
    }
}