// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApprenticeFeedbackDialogExtensions.cs" company="Education & Skills Funding Agency">
//   Copyright © 2018 Education & Skills Funding Agency
// </copyright>
// <summary>
//  Fluent extensions for building Bot Framework Dialogs.
//  TODO: replace with FormFlow once supported on Bot Framework V4
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ESFA.ProvideFeedback.Apprentice.Bot.Helpers
{
    using ESFA.ProvideFeedback.Apprentice.Bot.Services;

    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Prompts;

    /// <summary>
    /// Fluent extensions for building Bot Framework Dialogs quickly.
    /// TODO: replace with FormFlow once supported on Bot Framework V4
    /// </summary>
    public static class ApprenticeFeedbackDialogExtensions
    {
        /// <summary>
        /// Builds a new branching dialog with binary yes/no answers.
        /// </summary>
        /// <param name="dialogs"> The dialogs collection. </param>
        /// <param name="factory"> The factory used for building dialogs. </param>
        /// <param name="dialogName"> The name of the dialog to create. </param>
        /// <param name="prompt"> The prompt used for asking a question. </param>
        /// <param name="positivePath"> The next step in the event of a positive answer. </param>
        /// <param name="negativePath"> The next step in the event of a negative answer. </param>
        /// <returns> The <see cref="DialogSet"/>. </returns>
        public static DialogSet WithBranch(
            this DialogSet dialogs,
            IDialogFactory<DialogSet> factory,
            string dialogName,
            string prompt,
            IDialogStep positivePath,
            IDialogStep negativePath)
        {
            return factory.BuildBranchingDialog(dialogs, dialogName, prompt, positivePath, negativePath);
        }

        /// <summary>
        /// Builds a new binary choice prompt
        /// </summary>
        /// <param name="dialogs"> The dialogs collection. </param>
        /// <param name="factory"> The factory used for building dialogs. </param>
        /// <param name="promptName"> The name of the prompt, to be used in other dialogs. </param>
        /// <param name="listStyle"> The style of dialog options list. </param>
        /// <returns>
        /// The <see cref="DialogSet"/>.
        /// </returns>
        public static DialogSet WithChoicePrompt(
            this DialogSet dialogs,
            IDialogFactory<DialogSet> factory,
            string promptName,
            ListStyle listStyle = ListStyle.Auto)
        {
            return factory.BuildChoicePrompt(dialogs, promptName, listStyle);
        }

        /// <summary>
        /// Builds a dynamic ending dialog, based on previous dialog score.
        /// </summary>
        /// <param name="dialogs">The dialog collection</param>
        /// <param name="factory">The factory used to create dialogs</param>
        /// <param name="dialogName">The name of the dialog</param>
        /// <param name="requiredScore">The score required for a positive ending</param>
        /// <param name="positiveEnd">The next step in case of a positive ending</param>
        /// <param name="negativeEnd">The next step in case of a negative ending</param>
        /// <returns>
        /// The <see cref="DialogSet"/>.
        /// </returns>
        public static DialogSet WithDynamicEnd(
            this DialogSet dialogs,
            IDialogFactory<DialogSet> factory,
            string dialogName,
            int requiredScore,
            IDialogStep positiveEnd,
            IDialogStep negativeEnd)
        {
            return factory.BuildDynamicEndDialog(dialogs, dialogName, requiredScore, positiveEnd, negativeEnd);
        }

        /// <summary>
        /// Builds a free-text entry prompt
        /// </summary>
        /// <param name="dialogs">the dialog collection</param>
        /// <param name="factory">the factory used to create dialogs</param>
        /// <param name="dialogName">the name of the prompt</param>
        /// <param name="prompt">the text to display to a user</param>
        /// <param name="nextStep">the name of the step to proceed to</param>
        /// <returns>
        /// The <see cref="DialogSet"/>.
        /// </returns>
        public static DialogSet WithFreeTextEntry(
            this DialogSet dialogs,
            IDialogFactory<DialogSet> factory,
            string dialogName,
            string prompt,
            IDialogStep nextStep)
        {
            return factory.BuildFreeTextDialog(dialogs, dialogName, prompt, nextStep);
        }

        /// <summary>
        /// Builds a simple text prompt
        /// </summary>
        /// <param name="dialogs">the dialog collection</param>
        /// <param name="factory">the factory to be used for creating dialogs</param>
        /// <param name="promptName">the name of the prompt</param>
        /// <returns>
        /// The <see cref="DialogSet"/>.
        /// </returns>
        public static DialogSet WithTextPrompt(
            this DialogSet dialogs,
            IDialogFactory<DialogSet> factory,
            string promptName)
        {
            return factory.BuildTextPrompt(dialogs, promptName);
        }

        /// <summary>
        /// Builds a welcome dialog, shown on conversation start
        /// </summary>
        /// <param name="dialogs">the dialog collection</param>
        /// <param name="factory">the factory used to create dialogs</param>
        /// <param name="dialogName">the name of the dialog</param>
        /// <param name="nextStep">the next step to go to in the chain</param>
        /// <returns>
        /// The <see cref="DialogSet"/>.
        /// </returns>
        public static DialogSet WithWelcome(
            this DialogSet dialogs,
            IDialogFactory<DialogSet> factory,
            string dialogName,
            IDialogStep nextStep)
        {
            return factory.BuildWelcomeDialog(dialogs, dialogName, nextStep);
        }
    }
}