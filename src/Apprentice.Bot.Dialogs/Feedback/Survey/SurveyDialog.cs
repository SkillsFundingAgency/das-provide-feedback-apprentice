namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Feedback.Survey
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Feedback.Components;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Models;

    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Schema;

    /// <summary>
    /// Represents an end-to-end dynamic survey dialog.
    /// </summary>
    public class SurveyDialog : ComponentDialog
    {
        public SurveyDialog(string id)
            : base(id)
        {
            this.InitialDialogId = id;
        }

        public ICollection<ISurveyStep> Steps { get; protected set; }

        public SurveyDialog Build(DialogFactory factory)
        {
            WaterfallStep[] waterfall = this.Steps.Select(x => this.BuildDialog(x, factory)).ToArray();

            var dialog = new WaterfallDialog(this.Id, waterfall);

            this.AddDialog(dialog);

            return this;
        }

        public SurveyDialog WithSteps(ICollection<ISurveyStep> steps)
        {
            this.Steps = steps;
            return this;
        }

        /// <inheritdoc />
        /// This method is called when a child dialog is completed or otherwise terminated.
        /// Continue the conversation from here
        protected override async Task<DialogTurnResult> OnContinueDialogAsync(
            DialogContext innerDc,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var activity = innerDc.Context.Activity;

            switch (activity.Type)
            {
                case ActivityTypes.Message:
                    {
                        var result = await innerDc.ContinueDialogAsync(cancellationToken);

                        switch (result.Status)
                        {
                            case DialogTurnStatus.Empty:
                                {
                                    // await innerDc.Context.SendActivityAsync(MessageFactory.Text($"TURN EMPTY"),cancellationToken);
                                    break;
                                }

                            case DialogTurnStatus.Complete:
                                {
                                    // await innerDc.Context.SendActivityAsync(MessageFactory.Text($"TURN COMPLETE"),cancellationToken);

                                    // End active dialog.
                                    await innerDc.EndDialogAsync(cancellationToken: cancellationToken);
                                    break;
                                }

                            default:
                                {
                                    break;
                                }
                        }

                        break;
                    }

                case ActivityTypes.Event:
                    {
                        // await innerDc.Context.SendActivityAsync(MessageFactory.Text($"ONEVENT"), cancellationToken);
                        break;
                    }

                case ActivityTypes.ConversationUpdate:
                    {
                        // await innerDc.Context.SendActivityAsync(MessageFactory.Text($"ONSTART"), cancellationToken);
                        break;
                    }

                default:
                    {
                        // await innerDc.Context.SendActivityAsync(MessageFactory.Text($"DEFAULT"), cancellationToken);
                        break;
                    }
            }

            return EndOfTurn;
        }

        private WaterfallStep BuildDialog(ISurveyStep step, IDialogFactory dialogFactory)
        {
            ComponentDialog dialog;

            switch (step)
            {
                case StartStep startStep:
                    dialog = dialogFactory.Create<SurveyStartDialog>(startStep);
                    break;

                case BinaryQuestion binaryQuestionStep:
                    dialog = dialogFactory.Create<MultipleChoiceDialog>(binaryQuestionStep);
                    break;

                case FreeTextQuestion freeTextQuestionStep:
                    dialog = dialogFactory.Create<FreeTextDialog>(freeTextQuestionStep);
                    break;

                case EndStep endStep:
                    dialog = dialogFactory.Create<SurveyEndDialog>(endStep);
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"Unrecognized type [{step.GetType().FullName}]");
            }

            this.AddDialog(dialog);

            // return async (stepContext, cancellationToken) => await stepContext.ReplaceDialogAsync(dialog.Id, cancellationToken: cancellationToken);
            return async (stepContext, cancellationToken) =>
                await stepContext.BeginDialogAsync(dialog.Id, cancellationToken: cancellationToken);
        }
    }

    public enum InterruptionStatus
    {
        /// <summary>
        /// Indicates that the active dialog was interrupted and needs to resume.
        /// </summary>
        Interrupted,

        /// <summary>
        /// Indicates that there is a new dialog waiting and the active dialog needs to be shelved.
        /// </summary>
        Waiting,

        /// <summary>
        /// Indicates that no interruption action is required.
        /// </summary>
        NoAction,
    }
}