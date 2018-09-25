using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Commands
{
    public interface IBotDialogCommand
    {
        string Trigger { get; }

        Task ExecuteAsync(DialogContext dc);

        bool IsTriggered(DialogContext dc);
    }
}