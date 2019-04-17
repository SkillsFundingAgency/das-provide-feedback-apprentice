namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Builders
{
    using System;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Feedback.Components;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Interfaces;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Exceptions;
    using ESFA.DAS.ProvideFeedback.Apprentice.Services;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Extensions.Options;

    public class MultipleChoiceDialogComponentBuilder : ComponentBuilder<BinaryQuestion>
    {
        private readonly IFeedbackService feedbackService;

        public MultipleChoiceDialogComponentBuilder(IFeedbackBotStateRepository state, IOptions<Features> features, IOptions<Bot> botSettings, IFeedbackService feedbackService)
            : base(state, features, botSettings)
        {
            this.feedbackService = feedbackService;
        }

        public override ComponentDialog Create(ISurveyStepDefinition step)
        {
            try
            {
                if (step is BinaryQuestion questionStep)
                { 
                    return new MultipleChoiceDialog(questionStep.Id, this.State, this.BotSettings, this.Features, this.feedbackService)
                        .WithPrompt(questionStep.Prompt)
                        .WithResponses(questionStep.Responses)
                        .WithScore(questionStep.Score)
                        .Build();
                }

                throw new DialogFactoryException($"Could not create {nameof(MultipleChoiceDialog)}, expecting a {nameof(BinaryQuestion)} definition but was passed a {nameof(step.GetType)}");
            }
            catch (Exception ex)
            {
                throw new DialogFactoryException($"Error creating {nameof(MultipleChoiceDialog)}: {ex.Message}", ex);
            }
        }
    }
}