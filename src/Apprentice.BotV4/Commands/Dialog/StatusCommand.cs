namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Commands.Dialog
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Commands.Dialog;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;

    using Microsoft.Bot.Builder.Dialogs;

    using Newtonsoft.Json;

    using BotConfiguration = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Bot;

    public sealed class StatusCommand : AdminCommand, IBotDialogCommand
    {
        private readonly FeedbackBotStateRepository state;

        public StatusCommand(FeedbackBotStateRepository state, BotConfiguration botConfiguration)
            : base("bot_state_view", botConfiguration)
        {
            this.state = state ?? throw new ArgumentNullException(nameof(state));
        }

        public override async Task<DialogTurnResult> ExecuteAsync(DialogContext dialog, CancellationToken cancellationToken)
        {
            var reply = dialog.Context.Activity.CreateReply();

            UserProfile userProfile = await this.state.UserProfile.GetAsync(dialog.Context, () => new UserProfile(), cancellationToken);

            reply.Text = $"{JsonConvert.SerializeObject(userProfile, Formatting.None)}";

            await dialog.Context.SendActivityAsync(reply, cancellationToken: cancellationToken);

            return new DialogTurnResult(DialogTurnStatus.Waiting);
        }
    }
}