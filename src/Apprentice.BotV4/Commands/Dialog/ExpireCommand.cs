﻿namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Commands.Dialog
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Commands.Dialog;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;

    using Microsoft.Bot.Builder.Dialogs;

    using BotConfiguration = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Bot;

    public sealed class ExpireCommand : AdminCommand, IBotDialogCommand
    {
        private readonly IFeedbackBotStateRepository state;

        public ExpireCommand(IFeedbackBotStateRepository state, BotConfiguration botConfiguration)
            : base("bot--state--expire", botConfiguration)
        {
            this.state = state ?? throw new ArgumentNullException(nameof(state));
        }

        public override async Task<DialogTurnResult> ExecuteAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await this.state.UserProfile.GetAsync(dc.Context, () => new UserProfile(), cancellationToken);

            if (userProfile.SurveyState.StartDate != default(DateTime))
            {
                userProfile.SurveyState.StartDate =
                    userProfile.SurveyState.StartDate.AddHours(this.BotConfiguration.ConversationExpiryHours * -1);
            }

            await dc.Context.SendActivityAsync($"OK. Setting the conversation progress to 'expired' ", cancellationToken: cancellationToken);
            return new DialogTurnResult(DialogTurnStatus.Waiting);
        }
    }
}