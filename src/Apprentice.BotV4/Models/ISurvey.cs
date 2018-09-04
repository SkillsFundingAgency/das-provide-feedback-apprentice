namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models
{
    using System.Collections.Generic;

    public interface ISurvey
    {
        string Id { get; set; }

        ICollection<ISurveyStep> Steps { get; set; }
    }
}