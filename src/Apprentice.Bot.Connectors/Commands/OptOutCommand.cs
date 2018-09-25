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

        public override async Task ExecuteAsync(DialogContext dc)
        {
            // TODO: Add to suppression list here
            await dc.Context.SendActivity($"OK. You have opted out successfully.");
            dc.EndAll();
        }
    }
}