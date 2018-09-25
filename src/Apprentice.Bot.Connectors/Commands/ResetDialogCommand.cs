using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Commands
{
    public sealed class ResetDialogCommand : AdminCommand, IBotDialogCommand
    {

        public ResetDialogCommand()
            : base("reset")
        {
        }

        public override async Task ExecuteAsync(DialogContext dc)
        {
            UserInfo userInfo = UserState<UserInfo>.Get(dc.Context);
            userInfo.SurveyState = new SurveyState();

            ConversationInfo conversationInfo = ConversationState<ConversationInfo>.Get(dc.Context);
            conversationInfo.Clear();

            await dc.Context.SendActivity($"OK. Resetting conversation...");
            await dc.Continue();

        }
    }
}