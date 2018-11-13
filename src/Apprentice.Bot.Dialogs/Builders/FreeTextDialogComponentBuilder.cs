namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Builders
{
    using System;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Feedback.Components;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Interfaces;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Exceptions;

    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Extensions.Options;

    public class FreeTextDialogComponentBuilder : ComponentBuilder<FreeTextQuestion>
    {
        public FreeTextDialogComponentBuilder(FeedbackBotStateRepository state, IOptions<Features> features, IOptions<Bot> botSettings)
            : base(state, features, botSettings)
        {
        }

        public override ComponentDialog Create(ISurveyStepDefinition step)
        {
            try
            {
                if (step is FreeTextQuestion questionStep)
                {
                    return new FreeTextDialog(questionStep.Id, this.State, this.BotSettings, this.Features)
                        .WithPrompt(questionStep.Prompt)
                        .WithResponses(questionStep.Responses)
                        .WithScore(questionStep.Score)
                        .Build();
                }

                throw new DialogFactoryException($"Could not create {nameof(FreeTextDialog)}, expecting a {nameof(FreeTextQuestion)} definition but was passed a {nameof(step.GetType)}");
            }
            catch (Exception ex)
            {
                throw new DialogFactoryException($"Error creating {nameof(FreeTextDialog)}: {ex.Message}", ex);
            }
        }
    }
}