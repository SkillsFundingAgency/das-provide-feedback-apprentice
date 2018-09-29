namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Commands
{
    using System.Threading;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;

    using Microsoft.Bot.Builder.Dialogs;

    public sealed class ResetDialogCommand : AdminCommand, IBotDialogCommand
    {
        private readonly FeedbackBotState state;

        public ResetDialogCommand(FeedbackBotState state)
            : base("reset")
        {
            this.state = state;
        }

        public override async Task<DialogTurnResult> ExecuteAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            UserInfo userInfo = await this.state.UserInfo.GetAsync(dc.Context, () => new UserInfo(), cancellationToken);
            userInfo.SurveyState = new SurveyState();

            await this.state.ConversationState.ClearStateAsync(dc.Context, cancellationToken);

            await dc.Context.SendActivityAsync($"OK. Resetting conversation...", cancellationToken: cancellationToken);
            return await dc.ContinueDialogAsync(cancellationToken);
        }
    }
}