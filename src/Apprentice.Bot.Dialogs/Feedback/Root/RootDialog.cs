namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Feedback.Root
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;

    public class RootDialog : ComponentDialog
    {
        public const string PromptName = "mainMenuPrompt";

        private RootDialog()
            : base(nameof(RootDialog))
        {
            this.InitialDialogId = nameof(RootDialog);

            var steps = new WaterfallStep[]
                            {
                                this.MenuAsync,
                                this.StartAsync,
                                this.EndAsync,
                            };

            this.AddDialog(new WaterfallDialog(this.InitialDialogId, steps));
            this.AddDialog(new ChoicePrompt(PromptName));
        }

        public static RootDialog Instance { get; } = new RootDialog();

        private async Task<DialogTurnResult> MenuAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var menu = new List<string> { "start", "stop", "reset", "expire", "status" };
            return await stepContext.PromptAsync(PromptName, new PromptOptions() { Prompt = MessageFactory.Text("How can I help you?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> StartAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            // TODO: Add bot survey builder admin interface
            string result = stepContext.Context.Activity.Text.Trim().ToLowerInvariant();
            Dialog dialog = stepContext.Dialogs.Find(result);
            if (dialog != null)
            {
                await stepContext.BeginDialogAsync(result, cancellationToken: cancellationToken);
            }

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> EndAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}