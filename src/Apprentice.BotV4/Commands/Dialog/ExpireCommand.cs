namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Commands.Dialog
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
        private readonly FeedbackBotStateRepository state;
        private readonly BotConfiguration botConfiguration;

        public ExpireCommand(FeedbackBotStateRepository state, BotConfiguration botConfiguration)
            : base("expire", botConfiguration)
        {
            this.state = state ?? throw new ArgumentNullException(nameof(state));
            this.botConfiguration = botConfiguration;
        }

        public override async Task<DialogTurnResult> ExecuteAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await this.state.UserProfile.GetAsync(dc.Context, () => new UserProfile(), cancellationToken);

            if (userProfile.SurveyState.StartDate != default(DateTime))
            {
                userProfile.SurveyState.StartDate =
                    userProfile.SurveyState.StartDate.AddDays(this.botConfiguration.DefaultConversationExpiryDays * -1);
            }

            await dc.Context.SendActivityAsync($"OK. Setting the conversation progress to 'expired' ", cancellationToken: cancellationToken);
            return await dc.ContinueDialogAsync(cancellationToken);
        }
    }
}