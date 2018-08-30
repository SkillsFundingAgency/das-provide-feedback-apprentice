namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Commands
{
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.Bot.Builder.Core.Extensions;
    using Microsoft.Bot.Builder.Dialogs;

    using BotConfiguration = Infrastructure.Configuration.Bot;

    public class HelpCommand : UserCommand
    {
        private readonly BotConfiguration configuration;

        public HelpCommand(BotConfiguration configuration)
            : base("help")
        {
            this.configuration = configuration;
        }

        public override async Task ExecuteAsync(DialogContext dc)
        {
            // TODO: write some help text
            await dc.Context.SendActivity(MessageFactory.Text("help text goes here"));
        }
    }

    public class AdminHelpCommand : AdminCommand
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