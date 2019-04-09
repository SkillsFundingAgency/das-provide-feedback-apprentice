namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Builders
{

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Feedback.Components;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Interfaces;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Models;

    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Extensions.Options;

    using BotSettings = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Bot;
    using FeatureToggles = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Features;

    public class SurveyEndDialogComponentBuilder : ComponentBuilder<EndStepDefinition>
    {
        public SurveyEndDialogComponentBuilder(
            IFeedbackBotStateRepository state,
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