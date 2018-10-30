namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs
{
    using System;
    using System.Collections.Generic;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Feedback.Components;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Feedback.Survey;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Exceptions;
    using ESFA.DAS.ProvideFeedback.Apprentice.Data.Repositories;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Extensions.Options;

    using BotSettings = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Bot;
    using FeatureToggles = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Features;

    public class DialogFactory : IDialogFactory
    {
        private readonly FeedbackBotStateRepository state;

        private readonly BotSettings botSettings;

        private readonly FeatureToggles features;

        private readonly IFeedbackRepository feedbackRepository;

        /// <inheritdoc />
        public DialogFactory(FeedbackBotStateRepository state, IOptions<FeatureToggles> features, IOptions<BotSettings> botSettings, IFeedbackRepository feedbackRepository)
        {
            this.state = state ?? throw new ArgumentNullException(nameof(state));
            this.feedbackRepository = feedbackRepository;
            this.botSettings = botSettings.Value ?? throw new ArgumentNullException(nameof(botSettings));
            this.features = features.Value ?? throw new ArgumentNullException(nameof(features));
        }

        /// <inheritdoc />
        public T Create<T>(ISurveyStep step)
            where T : ComponentDialog => (T)this.Create(typeof(T), step);

        /// <inheritdoc />
        public T Create<T>(ISurvey survey)
            where T : ComponentDialog => (T)this.Create(typeof(T), survey);

        private ComponentDialog Create(Type type, ISurveyStep step)
        {
            if (type == typeof(MultipleChoiceDialog))
            {
                return this.CreateSurveyQuestionDialog(step);
            }

            if (type == typeof(FreeTextDialog))
            {
                return this.CreateSurveyQuestionDialog(step);
            }

            if (type == typeof(SurveyStartDialog))
            {
                return this.CreateSurveyStartDialog(step);
            }

            if (type == typeof(SurveyEndDialog))
            {
                return this.CreateSurveyEndDialog(step);
            }

            throw new DialogFactoryException($"Could not create DialogContainer : Unsupported type [{type.FullName}]");
        }

        private ComponentDialog Create(Type type, ISurvey survey)
        {
            if (type == typeof(SurveyDialog))
            {
                return this.CreateLinearSurveyDialog(survey);
            }

            throw new DialogFactoryException($"Could not create DialogContainer : Unsupported type [{type.FullName}]");
        }

        private SurveyDialog CreateLinearSurveyDialog(ISurvey survey)
        {
            var dialogs = new List<Dialog>();
            foreach (var step in survey.Steps)
            {
                switch (step)
                {
                    case StartStep startStep:
                        dialogs.Add(this.Create<SurveyStartDialog>(startStep)); 
                        break;

                    case EndStep endStep:
                        dialogs.Add(this.Create<SurveyEndDialog>(endStep));
                        break;

                    case BinaryQuestion binaryQuestionStep:
                        dialogs.Add(this.Create<MultipleChoiceDialog>(binaryQuestionStep));
                        break;

                    case FreeTextQuestion freeTextQuestionStep:
                        dialogs.Add(this.Create<FreeTextDialog>(freeTextQuestionStep));
                        break;

                    default:
                        throw new DialogFactoryException($"Could not create SurveyDialog step : Unsupported type [{step.GetType().FullName}]");
                }
            }

            return new SurveyDialog(survey.Id)
                .WithSteps(survey.Steps)
                .Build(this);
        }

        private ComponentDialog CreateSurveyQuestionDialog(ISurveyStep step)
        {
            switch (step)
            {
                case BinaryQuestion binaryQuestion:
                    return new MultipleChoiceDialog(step.Id, this.state, this.botSettings, this.features)
                        .WithPrompt(binaryQuestion.Prompt)
                        .WithResponses(binaryQuestion.Responses)
                        .WithScore(binaryQuestion.Score)
                        .Build();

                case FreeTextQuestion freeTextStep:
                    return new FreeTextDialog(step.Id, this.state, this.botSettings, this.features)
                        .WithPrompt(freeTextStep.Prompt)
                        .WithResponses(freeTextStep.Responses)
                        .Build();

                default:
                    throw new DialogFactoryException($"Could not create MultipleChoiceDialog : Unsupported type [{step.GetType().FullName}]");
            }
        }

        private ComponentDialog CreateSurveyStartDialog(ISurveyStep startStep)
        {
            return new SurveyStartDialog(startStep.Id, this.state, this.botSettings, this.features)
                .WithResponses(startStep.Responses)
                .Build();
        }

        private ComponentDialog CreateSurveyEndDialog(ISurveyStep endStep)
        {
            return new SurveyEndDialog(endStep.Id, this.state, this.botSettings, this.features, this.feedbackRepository)
                .WithResponses(endStep.Responses)
                .Build();
        }
    }
}