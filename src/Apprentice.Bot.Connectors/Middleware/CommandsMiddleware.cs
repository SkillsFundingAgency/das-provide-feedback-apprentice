﻿namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Middleware
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Commands.Dialog;

    using Microsoft.Bot.Builder;

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

        public async Task OnTurnAsync(
            ITurnContext turnContext,
            NextDelegate next,
            CancellationToken cancellationToken = new CancellationToken())
        {

        }
    }
}
