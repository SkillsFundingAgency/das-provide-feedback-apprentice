namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Commands
{
    using System.Threading.Tasks;

    using Microsoft.Bot.Builder.Dialogs;

    public interface IBotDialogCommand
    {
        string Trigger { get; }

        Task ExecuteAsync(DialogContext dc);

        bool IsTriggered(DialogContext dc);
    }
}