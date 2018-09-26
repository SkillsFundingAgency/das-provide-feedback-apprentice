using System.Threading;
using System;
using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Apprentice.BotV4;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;
using Microsoft.Bot.Builder.Dialogs;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Commands
{
    using BotConfiguration = Core.Configuration.Bot;

    public sealed class ExpireCommand : AdminCommand, IBotDialogCommand
    {
        private readonly FeedbackBotAccessors _accessors;
        private readonly BotConfiguration botConfiguration;

        public ExpireCommand(FeedbackBotAccessors accessors, BotConfiguration botConfiguration)
            : base("expire")
        {
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            this.botConfiguration = botConfiguration;
        }

        public override async Task<DialogTurnResult> ExecuteAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            UserInfo userInfo = await _accessors.UserProfile.GetAsync(dc.Context, () => new UserInfo(), cancellationToken);

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