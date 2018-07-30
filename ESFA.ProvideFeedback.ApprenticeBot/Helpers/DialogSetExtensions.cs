using System;
using ESFA.ProvideFeedback.ApprenticeBot.Services;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Prompts;

namespace ESFA.ProvideFeedback.ApprenticeBot.Helpers
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

        [Obsolete("Please build branching dialogs using WithWelcome method instead")]
        public static DialogSet WithRootDialog(this DialogSet dialogs, IDialogFactory<DialogSet> factory)
        {
            return factory.BuildApprenticeFeedbackRootDialog(dialogs);
        }
        [Obsolete("Please build branching dialogs using WithBranch method instead")]
        public static DialogSet WithQuestionSet(this DialogSet dialogs, IDialogFactory<DialogSet> factory)
        {
            return factory.BuildApprenticeFeedbackQuestionsDialog(dialogs);
        }
        [Obsolete("Please build free-text dialogs using WithFreeTextEntry method instead")]
        public static DialogSet WithAdditionalFeedback(this DialogSet dialogs, IDialogFactory<DialogSet> factory)
        {
            return factory.BuildApprenticeFeedbackAdditionalFeedbackDialog(dialogs);
        }
    }

    public static class DialogStepExtensions
    {
        public static IDialogStep WithResponse(this IDialogStep option, string response)
        {
            option.Responses.Add(response);
            return option;
        }
    }

}