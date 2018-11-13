namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Interfaces
{
    using System.Collections.Generic;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Models;

    public interface ISurveyDefinition
    {
        string Id { get; set; }

        ICollection<ISurveyStepDefinition> StepDefinitions { get; set; }
    }
}