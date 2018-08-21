namespace ESFA.ProvideFeedback.Apprentice.Bot.Services
{
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Prompts;

    /// <summary>
    /// Factory for adding conversational dialogs to Bots
    /// </summary>
    /// <typeparam name="T">
    /// The type of the dialog collection
    /// </typeparam>
    public interface IDialogFactory<T>
    {
        /// <summary>
        /// Build a new branching dialog. Usually used for binary yes/no type questions
        /// </summary>
        /// <param name="dialogs">the dialog collection</param>
        /// <param name="dialogName">the name of the dialog to build</param>
        /// <param name="prompt">the text prompt to show the user</param>
        /// <param name="positiveBranch">the next positive step</param>
        /// <param name="negativeBranch">the next negative step</param>
        /// <returns>The <see cref="DialogSet"/></returns>
        T BuildBranchingDialog(
            T dialogs,
            string dialogName,
            string prompt,
            IDialogStep positiveBranch,
            IDialogStep negativeBranch);

        /// <summary>
        /// Build a multi-choice prompt for a dialog question
        /// </summary>
        /// <param name="dialogs">the collection of dialogs</param>
        /// <param name="promptName">the text prompt to show the user</param>
        /// <param name="style">the style of list. This determines whether or not a list of options is presented to the user over the channel</param>
        /// <returns>The <see cref="DialogSet"/></returns>
        T BuildChoicePrompt(T dialogs, string promptName, ListStyle style);

        /// <summary>
        /// Builds a dynamic ending dialog, usually triggered by a certain score
        /// </summary>
        /// <param name="dialogs">the collection of dialogs to add to</param>
        /// <param name="dialogName">the name of the dialog</param>
        /// <param name="requiredScore">the required score for a positive result</param>
        /// <param name="positiveEnd">the positive step</param>
        /// <param name="negativeEnd">the negative step</param>
        /// <returns>The <see cref="DialogSet"/></returns>
        T BuildDynamicEndDialog(
            T dialogs,
            string dialogName,
            int requiredScore,
            IDialogStep positiveEnd,
            IDialogStep negativeEnd);

        /// <summary>
        /// Builds a free-text dialog for the user to enter anything in to
        /// </summary>
        /// <param name="dialogs">the dialog collection to add to</param>
        /// <param name="dialogName">the name of the dialog</param>
        /// <param name="prompt">the text prompt to show the user</param>
        /// <param name="nextStep">the next step in the chain</param>
        /// <returns>The <see cref="DialogSet"/></returns>
        T BuildFreeTextDialog(T dialogs, string dialogName, string prompt, IDialogStep nextStep);

        /// <summary>
        /// A simple text prompt requiring no input from the user
        /// </summary>
        /// <param name="dialogs">the collection of dialogs to add to</param>
        /// <param name="promptName">the prompt to display to the user</param>
        /// <returns>The <see cref="DialogSet"/></returns>  
        T BuildTextPrompt(T dialogs, string promptName);

        /// <summary>
        /// Builds a welcome dialog, usually shown when a conversation is initiated
        /// </summary>
        /// <param name="dialogs">the dialog collection to add to</param>
        /// <param name="dialogName">the name of the dialog</param>
        /// <param name="nextStep">the next step in the chain</param>
        /// <returns>The <see cref="DialogSet"/></returns>
        T BuildWelcomeDialog(T dialogs, string dialogName, IDialogStep nextStep);

        /// <summary>
        /// Build a new apprentice feedback dialog root
        /// </summary>
        /// <returns>
        /// The collection of dialogs.
        /// </returns>
        T Conversation();
    }
}