namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Builders
{
    using System;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Feedback.Components;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Interfaces;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.Services;

    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Extensions.Options;

    using BotSettings = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Bot;
    using FeatureToggles = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Features;

    public class SurveyEndDialogComponentBuilder : ComponentBuilder<EndStepDefinition>
    {
        public SurveyEndDialogComponentBuilder(
            FeedbackBotStateRepository state,
            IOptions<FeatureToggles> features,
            IOptions<BotSettings> botSettings)
            : base(state, features, botSettings)
        {
        }

        public override ComponentDialog Create(ISurveyStepDefinition stepDefinition)
        {
            return new SurveyEndDialog(stepDefinition.Id, this.State, this.BotSettings, this.Features)
                .WithResponses(stepDefinition.Responses)
                .Build();
        }
    }
}