namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Commands.Dialog
{
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Bot.Builder.Dialogs;

    public interface IBotDialogCommand
    {
        string Trigger { get; }

        Task<DialogTurnResult> ExecuteAsync(DialogContext dc, CancellationToken cancellationToken);

        bool IsTriggered(DialogContext dc);
    }
}