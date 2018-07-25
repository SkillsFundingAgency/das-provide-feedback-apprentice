using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace ESFA.ProvideFeedback.ApprenticeBot
{
    public class ApprenticeBot : IBot
    {
        /// <inheritdoc />
        public async Task OnTurn(ITurnContext context)
        {
            // This bot is only handling Messages
            if (context.Activity.Type == ActivityTypes.Message)
            {
                // Get the conversation state from the turn context
                var convoState = context.GetConversationState<ConvoState>();
                // var userState = context.GetUserState<UserState>();

                // Persist the latest message
                convoState.Messages.Add(context.Activity.Text);

                // Echo back to the user whatever they typed.
                await context.SendActivity($"Total of {convoState.Messages.Count} messages received. Latest: '{context.Activity.Text}'");
            }
        }
    }
}
