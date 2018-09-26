using System.Threading;

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Commands
{
    public sealed class OptOutCommand : UserCommand, IBotDialogCommand
    {
        public OptOutCommand()
            : base("stop")
        {   
        }

        public override async Task<DialogTurnResult> ExecuteAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            // TODO: Add to suppression list here
            await dc.Context.SendActivityAsync($"OK. You have opted out successfully.", cancellationToken: cancellationToken);
            return await dc.CancelAllDialogsAsync(cancellationToken);
        }
    }
}