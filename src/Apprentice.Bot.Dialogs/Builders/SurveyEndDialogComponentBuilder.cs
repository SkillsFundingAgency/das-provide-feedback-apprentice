﻿namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Builders
{

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
        private readonly IFeedbackService feedbackService;

        public SurveyEndDialogComponentBuilder(
            IFeedbackBotStateRepository state,
            IOptions<FeatureToggles> features,
            IOptions<BotSettings> botSettings,
            IFeedbackService feedbackService)
            : base(state, features, botSettings)
        {
            this.feedbackService = feedbackService;
        }

        public override ComponentDialog Create(ISurveyStepDefinition stepDefinition)
        {
            return new SurveyEndDialog(stepDefinition.Id, this.State, this.BotSettings, this.Features, this.feedbackService)
                .WithResponses(stepDefinition.Responses)
                .Build();
        }
    }
}