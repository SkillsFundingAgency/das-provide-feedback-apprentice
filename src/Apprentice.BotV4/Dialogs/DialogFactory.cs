using ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration;
using Microsoft.Extensions.Options;

namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Dialogs.Components;
    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Exceptions;

    using Microsoft.AspNetCore.Http;
    using Microsoft.Bot.Builder.Dialogs;

    public class DialogFactory : IDialogFactory
    {
        private readonly Features _features;

        public DialogFactory(IOptions<Features> features)
        {
            _features = features.Value ?? throw new ArgumentNullException(nameof(features));
        }

        public T Create<T>(ISurveyStep step)
            where T : DialogContainer
        {
            return (T)this.Create(typeof(T), step);
        }

        public T Create<T>(ISurvey survey)
            where T : DialogContainer
        {
            return (T)this.Create(typeof(T), survey);
        }

        public DialogContainer Create(Type type, ISurveyStep step)
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

        private DialogContainer Create(Type type, ISurvey survey)
        {
            if (type == typeof(LinearSurveyDialog))
            {
                return this.CreateLinearSurveyDialog(survey);
            }

            throw new DialogFactoryException($"Could not create DialogContainer : Unsupported type [{type.FullName}]");
        }

        private LinearSurveyDialog CreateLinearSurveyDialog(ISurvey survey)
        {
            var dialogs = new List<IDialog>();
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

        private SurveyQuestionDialog CreateSurveyQuestionDialog(ISurveyStep step)
        {
            switch (step)
            {
                case QuestionStep questionStep:
                    return new SurveyQuestionDialog(questionStep.Id)
                        .WithPrompt(questionStep.Prompt)
                        .WithResponses(questionStep.Responses)
                        .WithScore(questionStep.Score)
                        .Build();
                default:
                    throw new DialogFactoryException($"Could not create SurveyQuestionDialog : Unsupported type [{step.GetType().FullName}]");
            }
        }

        private SingleStepDialog CreateSingleStepDialog(ISurveyStep step)
        {
            switch (step)
            {
                case StartStep startStep:
                    return new SurveyStartDialog(startStep.Id)
                        .WithResponses(startStep.Responses)
                        .Build();
                case EndStep endStep:
                    return new SurveyEndDialog(endStep.Id)
                        .WithResponses(endStep.Responses)
                        .Build();
                default:
                    throw new DialogFactoryException($"Could not create SingleStepDialog : Unsupported type [{step.GetType().FullName}]");
            }
        }

        private SingleStepDialog CreateSurveyStartDialog(ISurveyStep startStep)
        {
            return new SurveyStartDialog(startStep.Id)
                .WithResponses(startStep.Responses)
                .Build();
        }

        private SingleStepDialog CreateSurveyEndDialog(ISurveyStep endStep)
        {
            return new SurveyEndDialog(endStep.Id)
                .WithResponses(endStep.Responses)
                .Build();
        }
    }
}