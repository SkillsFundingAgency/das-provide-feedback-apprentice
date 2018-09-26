using System.Collections.Generic;
using System.Threading;
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

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next,
            CancellationToken cancellationToken = new CancellationToken())
        {
            await next(cancellationToken);
        }
    }
}
