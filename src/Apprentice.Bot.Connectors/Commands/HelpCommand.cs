﻿namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Commands
{
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;

    public class HelpCommand : UserCommand, IBotDialogCommand
    {
        private readonly Core.Configuration.Bot configuration;

        public HelpCommand(Core.Configuration.Bot configuration)
            : base("help")
        {
            this.configuration = configuration;
        }

        public override async Task<DialogTurnResult> ExecuteAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            // TODO: write some help text
            await dc.Context.SendActivityAsync(MessageFactory.Text("help text goes here"), cancellationToken);
            return await dc.ContinueDialogAsync(cancellationToken);
        }
    }
}