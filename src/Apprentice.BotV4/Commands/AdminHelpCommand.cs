namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Commands
{
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.Bot.Builder.Core.Extensions;
    using Microsoft.Bot.Builder.Dialogs;

    using BotConfiguration = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Bot;

    public class AdminHelpCommand : AdminCommand, IBotDialogCommand
    {
        private readonly BotConfiguration configuration;

        public AdminHelpCommand(BotConfiguration configuration)
            : base("admin")
        {
            this.configuration = configuration;
        }

        public override async Task ExecuteAsync(DialogContext dc)
        {
            var menu = this.configuration.AdminCommands;
            if (menu.Any())
            {
                await dc.Context.SendActivity(MessageFactory.SuggestedActions(menu, "Administrative tasks available:"));
            }
        }
    }
}