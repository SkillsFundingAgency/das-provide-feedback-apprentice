namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;

    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Schema;

    using Newtonsoft.Json;

    public sealed class StatusCommand : AdminCommand, IBotDialogCommand
    {
        private readonly FeedbackBotStateRepository state;

        public StatusCommand(FeedbackBotStateRepository state)
            : base("status")
        {
            this.state = state ?? throw new ArgumentNullException(nameof(state));
        }

        public override async Task<DialogTurnResult> ExecuteAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await this.state.UserProfile.GetAsync(dc.Context, () => new UserProfile(), cancellationToken);
            await dc.Context.SendActivityAsync($"{JsonConvert.SerializeObject(userProfile, Formatting.Indented )}", cancellationToken: cancellationToken);

            DialogState dialogState = await this.state.ConversationDialogState.GetAsync(dc.Context, () => new DialogState(), cancellationToken);
            await dc.Context.SendActivityAsync($"{JsonConvert.SerializeObject(dialogState, Formatting.Indented)}", cancellationToken: cancellationToken);

            return await dc.ContinueDialogAsync(cancellationToken);
        }
    }
}