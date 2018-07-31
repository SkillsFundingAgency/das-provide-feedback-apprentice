using Microsoft.Bot.Builder.Prompts;

namespace ESFA.ProvideFeedback.Apprentice.Bot.Services
{
    public interface IDialogFactory<T>
    {
        T BuildApprenticeFeedbackDialog();

        T BuildWelcomeDialog(T dialogs, string dialogName, IDialogStep nextStep);
        T BuildBranchingDialog(T dialogs, string dialogName, string prompt, IDialogStep positiveBranch, IDialogStep negativeBranch);
        T BuildFreeTextDialog(T dialogs, string dialogName, string prompt, IDialogStep nextStep);
        T BuildDynamicEndDialog(T dialogs, string dialogName, int requiredScore, IDialogStep positiveEnd, IDialogStep negativeEnd);
        T BuildChoicePrompt(T dialogs, string promptName, ListStyle style);
        T BuildTextPrompt(T dialogs, string promptName);
    }
}