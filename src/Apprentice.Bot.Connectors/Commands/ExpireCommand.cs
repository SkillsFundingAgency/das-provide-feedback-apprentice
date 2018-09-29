using System.Threading;
using System;
using System.Threading.Tasks;

using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;
using Microsoft.Bot.Builder.Dialogs;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Commands
{
    using BotConfiguration = Core.Configuration.Bot;

    public sealed class ExpireCommand : AdminCommand, IBotDialogCommand
    {
        private readonly FeedbackBotState state;
        private readonly BotConfiguration botConfiguration;

        public ExpireCommand(FeedbackBotState state, BotConfiguration botConfiguration)
            : base("expire")
        {
            this.state = state ?? throw new ArgumentNullException(nameof(state));
            this.botConfiguration = botConfiguration;
        }

        public override async Task<DialogTurnResult> ExecuteAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            UserInfo userInfo = await this.state.UserInfo.GetAsync(dc.Context, () => new UserInfo(), cancellationToken);

            if (userInfo.SurveyState.StartDate != default(DateTime))
            {
                userInfo.SurveyState.StartDate =
                    userInfo.SurveyState.StartDate.AddDays(this.botConfiguration.DefaultConversationExpiryDays * -1);
            }

            await dc.Context.SendActivityAsync($"OK. Setting the conversation progress to 'expired' ", cancellationToken: cancellationToken);
            return await dc.ContinueDialogAsync(cancellationToken);
        }
    }
}