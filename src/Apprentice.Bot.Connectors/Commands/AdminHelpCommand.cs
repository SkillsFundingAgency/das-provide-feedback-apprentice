using System.Threading;
using Microsoft.Bot.Builder;

using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Commands
{
    using BotConfiguration = Core.Configuration.Bot;

    public class AdminHelpCommand : AdminCommand, IBotDialogCommand
    {
        private readonly BotConfiguration configuration;

        public AdminHelpCommand(BotConfiguration configuration)
            : base("admin")
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