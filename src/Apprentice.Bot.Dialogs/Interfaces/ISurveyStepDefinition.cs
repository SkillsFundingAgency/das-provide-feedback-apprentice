﻿namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Interfaces
{
    using System.Collections.Generic;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Models;

    public interface ISurveyStepDefinition
    {
        string Id { get; set; }

        bool IsValid { get; set; }

        int Order { get; set; }

        ICollection<IBotResponse> Responses { get; set; }
    }
}