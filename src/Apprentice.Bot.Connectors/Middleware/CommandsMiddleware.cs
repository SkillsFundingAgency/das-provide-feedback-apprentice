using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Commands;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Middleware
{
    /// <inheritdoc />
    public class CommandsMiddleware : IMiddleware
    {
        private readonly IEnumerable<IBotDialogCommand> commands;

        public CommandsMiddleware(IEnumerable<IBotDialogCommand> commands)
        {
            this.commands = commands;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="CommandsMiddleware"/> class. 
        /// </summary>
        ~CommandsMiddleware()
        {
            // cleanup
        }

        /// <inheritdoc />
        public async Task OnTurn(ITurnContext context, MiddlewareSet.NextDelegate next)
        {
            UserInfo userInfo = UserState<UserInfo>.Get(context);
            ConversationInfo conversationInfo = ConversationState<ConversationInfo>.Get(context);

            await next();
        }
    }
}
