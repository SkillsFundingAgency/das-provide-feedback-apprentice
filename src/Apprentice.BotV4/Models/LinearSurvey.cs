namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models
{
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    /// <summary>
    /// Represents a survey that has a start, sequential questions, and an end.
    /// </summary>
    [Serializable]
    public class LinearSurvey : ISurvey
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("steps")]
        public ICollection<ISurveyStep> Steps { get; set; } = new List<ISurveyStep>();
    }
}