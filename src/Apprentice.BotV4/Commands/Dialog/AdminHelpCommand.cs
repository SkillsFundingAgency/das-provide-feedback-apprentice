namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Commands.Dialog
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Commands.Dialog;

    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;

    using BotConfiguration = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Bot;

    public class AdminHelpCommand : AdminCommand, IBotDialogCommand
    {
        private readonly BotConfiguration configuration;

        public AdminHelpCommand(BotConfiguration configuration)
            : base("admin", configuration)
        {
            this.configuration = configuration;
        }

        public override async Task<DialogTurnResult> ExecuteAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            var menu = this.configuration.AdminCommands;
            if (menu.Any())
            {
                await dc.Context.SendActivityAsync(MessageFactory.SuggestedActions(menu, "Administrative tasks available:"), cancellationToken);
            }

            return await dc.ContinueDialogAsync(cancellationToken);
        }
    }
}