using System.Threading.Tasks;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Commands
{
    public class HelpCommand : UserCommand, IBotDialogCommand
    {
        private readonly Core.Configuration.Bot configuration;

        public HelpCommand(Core.Configuration.Bot configuration)
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
}