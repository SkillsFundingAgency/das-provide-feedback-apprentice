using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;

namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Commands
{
    using System;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.Bot.Builder.Core.Extensions;
    using Microsoft.Bot.Builder.Dialogs;

    using Newtonsoft.Json;

    public sealed class StatusCommand : AdminCommand, IBotDialogCommand
    {
        public StatusCommand()
            : base("status")
        {
        }

        public override async Task ExecuteAsync(DialogContext dc)
        {
            UserInfo userInfo = UserState<UserInfo>.Get(dc.Context);
            await dc.Context.SendActivity($"{JsonConvert.SerializeObject(userInfo, Formatting.Indented )}");

            ConversationInfo state = ConversationState<ConversationInfo>.Get(dc.Context);
            await dc.Context.SendActivity($"{JsonConvert.SerializeObject(state, Formatting.Indented)}");
        }
    }
}