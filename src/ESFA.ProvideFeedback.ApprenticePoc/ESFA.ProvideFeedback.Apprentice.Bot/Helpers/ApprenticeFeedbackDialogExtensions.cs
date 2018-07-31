using ESFA.ProvideFeedback.Apprentice.Bot.Services;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Prompts;

namespace ESFA.ProvideFeedback.Apprentice.Bot.Helpers
{
    public static class ApprenticeFeedbackDialogExtensions
    {
        public static DialogSet WithChoicePrompt(this DialogSet dialogs, IDialogFactory<DialogSet> factory,
            string promptName, ListStyle style = ListStyle.Auto)
        {
            return factory.BuildChoicePrompt(dialogs, promptName, style);
        }

        public static DialogSet WithTextPrompt(this DialogSet dialogs, IDialogFactory<DialogSet> factory,
            string promptName)
        {
            return factory.BuildTextPrompt(dialogs, promptName);
        }

        public static DialogSet WithWelcome(this DialogSet dialogs, IDialogFactory<DialogSet> factory,
            string dialogName, IDialogStep nextStep)
        {
            return factory.BuildWelcomeDialog(dialogs, dialogName, nextStep);
        }

        public static DialogSet WithBranch(this DialogSet dialogs, IDialogFactory<DialogSet> factory, string dialogName, string prompt, IDialogStep positivePath, IDialogStep negativePath)
        {
            return factory.BuildBranchingDialog(dialogs, dialogName, prompt, positivePath, negativePath);
        }

        public static DialogSet WithFreeTextEntry(this DialogSet dialogs, IDialogFactory<DialogSet> factory,
            string dialogName, string prompt, IDialogStep nextStep)
        {
            return factory.BuildFreeTextDialog(dialogs, dialogName, prompt, nextStep);
        }

        public static DialogSet WithDynamicEnd(this DialogSet dialogs, IDialogFactory<DialogSet> factory,
            string dialogName, int requiredScore, IDialogStep positiveEnd, IDialogStep negativeEnd)
        {
            return factory.BuildDynamicEndDialog(dialogs, dialogName, requiredScore, positiveEnd, negativeEnd);
        }
    }
}