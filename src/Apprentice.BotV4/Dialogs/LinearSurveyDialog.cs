namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Dialogs.Components;
    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models;

    using Microsoft.Bot.Builder.Dialogs;

    public class LinearSurveyDialog : DialogContainer
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

            this.Dialogs.Add(this.DialogId, waterfall);

            return this;
        }

        public LinearSurveyDialog WithSteps(ICollection<ISurveyStep> steps)
        {
            this.Steps = steps;
            return this;
        }

        private WaterfallStep BuildDialog(ISurveyStep step, IDialogFactory dialogFactory)
        {
            DialogContainer dialog;

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

            this.RegisterDialog(step.Id, dialog);
            return async (dc, args, next) => { await dc.Begin(step.Id); };
        }

        private void RegisterDialog(string dialogId, IDialog dialog)
        {
            this.Dialogs.Add(dialogId, dialog);
        }
    }
}