﻿namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Commands.Dialog
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Commands.Dialog;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Conversation;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;

    using Microsoft.Bot.Builder.Dialogs;

    public sealed class OptOutCommand : UserCommand, IBotDialogCommand
    {
        private readonly IFeedbackBotStateRepository state;

        public OptOutCommand(IFeedbackBotStateRepository state)
            : base("stop")
        {
            this.state = state ?? throw new ArgumentNullException(nameof(state));
        }

        public override async Task<DialogTurnResult> ExecuteAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await this.state.UserProfile.GetAsync(dc.Context, () => new UserProfile(), cancellationToken);

            userProfile.SurveyState.EndDate = DateTime.UtcNow;
            userProfile.SurveyState.Progress = ProgressState.OptedOut;

            // TODO: Add to suppression list here
            await dc.Context.SendActivityAsync($"OK. You have opted out successfully.", cancellationToken: cancellationToken);
            return await dc.CancelAllDialogsAsync(cancellationToken);
        }
    }
}