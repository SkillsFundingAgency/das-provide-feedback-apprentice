namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors;
    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Dialogs.Components;
    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Exceptions;

    using Microsoft.AspNetCore.Http;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Extensions.Options;

    using AzureSettings = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Azure;
    using BotSettings = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Bot;
    using DataSettings = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Data;
    using NotifySettings = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Notify;
    using FeatureToggles = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Features;

    public class DialogFactory : IDialogFactory
    {
        private readonly FeedbackBotState state;

        private readonly BotSettings botSettings;

        private readonly FeatureToggles features;

        /// <inheritdoc />
        public DialogFactory(FeedbackBotState state, IOptions<FeatureToggles> features, IOptions<BotSettings> botSettings)
        {
            this.state = state ?? throw new ArgumentNullException(nameof(state));
            this.botSettings = botSettings.Value ?? throw new ArgumentNullException(nameof(botSettings));
            this.features = features.Value ?? throw new ArgumentNullException(nameof(features));
        }

        /// <inheritdoc />
        public T Create<T>(ISurveyStep step)
            where T : ComponentDialog => (T)this.Create(typeof(T), step);

        /// <inheritdoc />
        public T Create<T>(ISurvey survey)
            where T : ComponentDialog => (T)this.Create(typeof(T), survey);

        public ComponentDialog Create(Type type, ISurveyStep step)
        {
            if (type == typeof(SurveyQuestionDialog))
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
            if (type == typeof(LinearSurveyDialog))
            {
                return this.CreateLinearSurveyDialog(survey);
            }

            throw new DialogFactoryException($"Could not create DialogContainer : Unsupported type [{type.FullName}]");
        }

        private LinearSurveyDialog CreateLinearSurveyDialog(ISurvey survey)
        {
            var dialogs = new List<Dialog>();
            foreach (var s in survey.Steps)
            {
                switch (s)
                {
                    case StartStep startStep:
                        dialogs.Add(this.Create<SurveyStartDialog>(startStep)); 
                        break;

                    case EndStep endStep:
                        dialogs.Add(this.Create<SurveyEndDialog>(endStep));
                        break;

                    case QuestionStep questionStep:
                        dialogs.Add(this.Create<SurveyQuestionDialog>(questionStep));
                        break;

                    default:
                        throw new DialogFactoryException($"Could not create LinearSurveyDialog step : Unsupported type [{s.GetType().FullName}]");
                }
            }

            return new LinearSurveyDialog(survey.Id)
                .WithSteps(survey.Steps)
                .Build(this);
        }

        private ComponentDialog CreateSurveyQuestionDialog(ISurveyStep step)
        {
            switch (step)
            {
                case QuestionStep questionStep:
                    return new SurveyQuestionDialog(this.state, this.botSettings, this.features)
                        .WithPrompt(questionStep.Prompt)
                        .WithResponses(questionStep.Responses)
                        .WithScore(questionStep.Score)
                        .Build(step.Id);
                default:
                    throw new DialogFactoryException($"Could not create SurveyQuestionDialog : Unsupported type [{step.GetType().FullName}]");
            }
        }

        private ComponentDialog CreateSurveyStartDialog(ISurveyStep startStep)
        {
            return new SurveyStartDialog(this.state, this.botSettings, this.features)
                .WithResponses(startStep.Responses)
                .Build(startStep.Id);
        }

        private ComponentDialog CreateSurveyEndDialog(ISurveyStep endStep)
        {
            return new SurveyEndDialog(this.state, this.botSettings, this.features)
                .WithResponses(endStep.Responses)
                .Build(endStep.Id);
        }
    }
}