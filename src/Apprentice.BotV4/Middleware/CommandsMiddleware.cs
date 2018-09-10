using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;

namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Middleware
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Commands;
    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models;


    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Core.Extensions;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Schema;

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
