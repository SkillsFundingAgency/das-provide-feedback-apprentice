namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Commands
{
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration;

    using Microsoft.Bot.Builder.Core.Extensions;
    using Microsoft.Bot.Builder.Dialogs;

    public class HelpCommand : UserCommand
    {
        private readonly Bot configuration;

        public HelpCommand(Bot configuration)
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