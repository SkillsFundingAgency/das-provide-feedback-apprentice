namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Dialogs
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Recognizers.Text;

    using ChoicePrompt = Microsoft.Bot.Builder.Dialogs.ChoicePrompt;
    using TextPrompt = Microsoft.Bot.Builder.Dialogs.TextPrompt;

    public class RootDialog : ComponentDialog
    {
        public const string Id = "root-dialog";

        private RootDialog()
            : base(Id)
        {
            var steps = new WaterfallStep[]
                            {
                                this.MenuAsync,
                                this.StartAsync,
                                this.EndAsync,
                            };
        }

        public static RootDialog Instance { get; } = new RootDialog();

        private async Task<DialogTurnResult> MenuAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var menu = new List<string> { "start", "stop", "reset", "expire", "status" };
            await stepContext.Context.SendActivityAsync(MessageFactory.SuggestedActions(menu, "How can I help you?"), cancellationToken);

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
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