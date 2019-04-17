namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Builders
{
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Feedback.Components;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Interfaces;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration;

    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Extensions.Options;

    public class SurveyStartDialogComponentBuilder : ComponentBuilder<StartStepDefinition>
    {
        public SurveyStartDialogComponentBuilder(IFeedbackBotStateRepository state, IOptions<Features> features, IOptions<Bot> botSettings)
            : base(state, features, botSettings)
        {
        }

        public override ComponentDialog Create(ISurveyStepDefinition stepDefinition)
        {
            return new SurveyStartDialog(stepDefinition.Id, this.State, this.BotSettings, this.Features)
                .WithResponses(stepDefinition.Responses)
                .Build();
        }
    }
}