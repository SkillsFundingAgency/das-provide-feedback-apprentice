namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Models
{
    using System.Collections.Generic;

    public interface ISurvey
    {
        string Id { get; set; }

        ICollection<ISurveyStepDefinition> Steps { get; set; }
    }
}