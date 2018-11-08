namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs
{
    using System;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Models;

    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Extensions.Options;

    using BotSettings = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Bot;
    using FeatureToggles = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Features;

    public interface IComponentBuilder<out TDialog>
    {
        bool Matches(ISurveyStepDefinition stepDefinition);

        TDialog Create(ISurveyStepDefinition stepDefinition);
    }

    public abstract class ComponentBuilder<TDefinition> : IComponentBuilder<ComponentDialog>
    {
        protected readonly BotSettings BotSettings;

        protected readonly FeatureToggles Features;

        protected readonly FeedbackBotStateRepository State;

        protected ComponentBuilder(
            FeedbackBotStateRepository state,
            IOptions<FeatureToggles> features,
            IOptions<BotSettings> botSettings)
        {
            this.BotSettings = botSettings.Value ?? throw new ArgumentNullException(nameof(botSettings));
            this.Features = features.Value ?? throw new ArgumentNullException(nameof(features));
            this.State = state ?? throw new ArgumentNullException(nameof(state));
        }

        public bool Matches(ISurveyStepDefinition stepDefinition)
        {
            return stepDefinition is TDefinition;
        }

        public abstract ComponentDialog Create(ISurveyStepDefinition stepDefinition);
    }
}
