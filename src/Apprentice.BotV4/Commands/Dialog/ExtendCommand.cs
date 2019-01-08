namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Commands.Dialog
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Commands.Dialog;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Conversation;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;

    using Microsoft.Bot.Builder.Dialogs;

    public sealed class ExtendCommand : AdminCommand, IBotDialogCommand
    {
        private readonly FeedbackBotStateRepository state;

        public ExtendCommand(FeedbackBotStateRepository state, Bot botConfiguration)
            : base("bot_state_extend", botConfiguration)
        {
            this.state = state ?? throw new ArgumentNullException(nameof(state));
        }

        public override async Task<DialogTurnResult> ExecuteAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await this.state.UserProfile.GetAsync(dc.Context, () => new UserProfile(), cancellationToken);

            if (userProfile.SurveyState.StartDate != default(DateTime))
            {
                userProfile.SurveyState.StartDate = DateTime.UtcNow;
                userProfile.SurveyState.Progress = ProgressState.InProgress;
            }

            await dc.Context.SendActivityAsync($"OK. Resetting the conversation expiry ", cancellationToken: cancellationToken);
            return new DialogTurnResult(DialogTurnStatus.Waiting);
        }
    }
}