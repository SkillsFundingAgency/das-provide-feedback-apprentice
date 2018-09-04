namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models
{
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public interface ISurvey
    {
        string Id { get; set; }

        ICollection<ISurveyStep> Steps { get; set; }
    }

    public interface ISurveyStep
    {
        string Id { get; set; }

        bool IsValid { get; set; }

        int Order { get; set; }

        ICollection<IResponse> Responses { get; set; }
    }

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

    [Serializable]
    public sealed class StartStep : SurveyStepBase
    {
    }

    [Serializable]
    public sealed class EndStep : SurveyStepBase
    {
    }

    [Serializable]
    public class QuestionStep : SurveyStepBase
    {
        [JsonProperty("prompt")]
        public string Prompt { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }
    }

    [Serializable]
    public sealed class BinaryQuestion : QuestionStep
    {
    }
}