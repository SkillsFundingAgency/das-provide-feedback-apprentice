namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Commands.Dialog
{
    using System.Threading;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Commands.Dialog;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;

    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Schema;
    using BotConfiguration = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Bot;

    public sealed class ResetDialogCommand : AdminCommand, IBotDialogCommand
    {
        private readonly IFeedbackBotStateRepository state;

        public ResetDialogCommand(IFeedbackBotStateRepository state, BotConfiguration botConfiguration)
            : base("bot_state_reset", botConfiguration)
        {
            this.state = state;
        }

        public override async Task<DialogTurnResult> ExecuteAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await this.state.UserProfile.GetAsync(dc.Context, () => new UserProfile(), cancellationToken);
            userProfile.SurveyState = new SurveyState();

            await this.state.ConversationState.ClearStateAsync(dc.Context, cancellationToken);
            var activity = new Activity { Id = "Reset", Text = "OK. Resetting conversation...", Type = ActivityTypes.Message };
            await dc.Context.SendActivityAsync(activity, cancellationToken: cancellationToken);
            return await dc.CancelAllDialogsAsync(cancellationToken);
        }
    }
}