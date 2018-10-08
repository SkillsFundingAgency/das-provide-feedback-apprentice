namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Commands.Dialog
{
    using System.Threading;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;

    using Microsoft.Bot.Builder.Dialogs;

    public sealed class ResetDialogCommand : AdminCommand, IBotDialogCommand
    {
        private readonly FeedbackBotStateRepository state;

        public ResetDialogCommand(FeedbackBotStateRepository state)
            : base("reset")
        {
            this.state = state;
        }

        public override async Task<DialogTurnResult> ExecuteAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await this.state.UserProfile.GetAsync(dc.Context, () => new UserProfile(), cancellationToken);
            userProfile.SurveyState = new SurveyState();

            await this.state.ConversationState.ClearStateAsync(dc.Context, cancellationToken);

            await dc.Context.SendActivityAsync($"OK. Resetting conversation...", cancellationToken: cancellationToken);
            return await dc.ContinueDialogAsync(cancellationToken);
        }
    }
}