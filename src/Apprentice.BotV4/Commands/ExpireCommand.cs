namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Commands
{
    using System;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.State;

    using Microsoft.Bot.Builder.Core.Extensions;
    using Microsoft.Bot.Builder.Dialogs;

    using BotConfiguration = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Bot;

    public sealed class ExpireCommand : AdminCommand
    {
        private readonly BotConfiguration botConfiguration;

        public ExpireCommand(BotConfiguration botConfiguration)
            : base("expire")
        {
            this.botConfiguration = botConfiguration;
        }

        public override async Task ExecuteAsync(DialogContext dc)
        {
            UserInfo userInfo = UserState<UserInfo>.Get(dc.Context);

            if (userInfo.SurveyState.StartDate != default(DateTime))
            {
                userInfo.SurveyState.StartDate =
                    userInfo.SurveyState.StartDate.AddDays(this.botConfiguration.DefaultConversationExpiryDays * -1);
            }

            await dc.Context.SendActivity($"OK. Setting the conversation progress to 'expired' ");
        }
    }
}