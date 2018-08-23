using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Middleware
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;

    using global::ESFA.DAS.ProvideFeedback.Apprentice.BotV4.State;

    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Core.Extensions;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Schema;

    /// <inheritdoc />
    public class CommandsMiddleware : IMiddleware
    {
        /// <summary>
        /// Finalizes an instance of the <see cref="CosmosConversationLog"/> class. 
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
