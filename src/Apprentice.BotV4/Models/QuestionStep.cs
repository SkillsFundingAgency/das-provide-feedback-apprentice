namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models
{
    using System;

    using Newtonsoft.Json;

    [Serializable]
    public class QuestionStep : SurveyStepBase
    {
        [JsonProperty("prompt")]
        public string Prompt { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }
    }
}