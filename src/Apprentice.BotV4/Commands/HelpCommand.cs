using System.Threading;
using Microsoft.Bot.Builder;

namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Commands
{
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration;

    using Microsoft.Bot.Builder.Dialogs;

    public class HelpCommand : UserCommand
    {
        private readonly Bot configuration;

        public HelpCommand(Bot configuration)
            : base("help")
        {
            this.configuration = configuration;
        }

        public override async Task<DialogTurnResult> ExecuteAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            // TODO: write some help text
            await dc.Context.SendActivityAsync(MessageFactory.Text("help text goes here"), cancellationToken);
            return await dc.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}