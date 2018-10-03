namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Dialogs.Components;
    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models;

    using Microsoft.Bot.Builder.Dialogs;

    public class LinearSurveyDialog : ComponentDialog
    {
        public LinearSurveyDialog(string id)
            : base(id)
        {
        }

        public ICollection<ISurveyStep> Steps { get; protected set; }

        public LinearSurveyDialog Build(DialogFactory factory)
        {
            WaterfallStep[] waterfall =
                this.Steps.Select(x => this.BuildDialog(x, factory)).ToArray();

            var dialog = new WaterfallDialog(this.Id, waterfall);

            this.AddDialog(dialog);

            return this;
        }

        public LinearSurveyDialog WithSteps(ICollection<ISurveyStep> steps)
        {
            this.Steps = steps;
            return this;
        }

        private WaterfallStep BuildDialog(ISurveyStep step, IDialogFactory dialogFactory)
        {
            ComponentDialog dialog;

            switch (step)
            {
                case StartStep startStep:
                    dialog = dialogFactory.Create<SurveyStartDialog>(startStep);

                    break;

                case EndStep endStep:
                    dialog = dialogFactory.Create<SurveyEndDialog>(endStep);
                    break;

                case QuestionStep questionStep:
                    dialog = dialogFactory.Create<SurveyQuestionDialog>(questionStep);
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"Unrecognized type [{step.GetType().FullName}]");
            }

            this.AddDialog(dialog);

            return async (stepContext, cancellationToken) => await stepContext.BeginDialogAsync(step.Id, cancellationToken: cancellationToken);
        }
    }
}