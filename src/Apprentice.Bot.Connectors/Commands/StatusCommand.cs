using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Commands
{
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