// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FormHelper.cs" company="Education & Skills Funding Agency">
//   Copyright © 2018 Education & Skills Funding Agency
// </copyright>
// <summary>
//   Provides a number of helper methods for creating Bot Framework forms
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ESFA.ProvideFeedback.Apprentice.Bot.Helpers
{
    using System.Collections.Generic;

    using ESFA.ProvideFeedback.Apprentice.Bot.Services;

    using Microsoft.Bot.Builder.Core.Extensions;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Prompts.Choices;
    using Microsoft.Bot.Schema;

    /// <summary>
    /// Provides a number of helper methods for creating Bot Framework forms
    /// </summary>
    public static class FormHelper
    {
        /// <summary>
        /// Creates an options list for binary yes/no confirmation prompts.
        /// </summary>
        public static ChoicePromptOptions ConfirmationPromptOptions =>
            new ChoicePromptOptions()
                {
                    Choices = BuildConfirmationChoices(),
                    RetryPromptActivity = MessageFactory.Text(
                        "Sorry, I'm just a simple bot. Please type ‘Yes’ or ‘No’",
                        inputHint: InputHints.ExpectingInput),
                };

        /// <summary>
        /// Builds a dynamic option for ending a conversation.
        /// </summary>
        /// <returns>see <see cref="IDialogStep"/></returns>
        public static IDialogStep BuildConversationEndOption()
        {
            return new ConversationEndOption { DialogTarget = null, Responses = new List<string>() };
        }

        /// <summary>
        /// Builds a conversation branch path
        /// </summary>
        /// <param name="target">The name of the dialog to proceed to</param>
        /// <returns>see <see cref="IDialogStep"/></returns>
        public static IDialogStep BuildConversationPath(string target)
        {
            return new DialogBranchOption { DialogTarget = target, Responses = new List<string>() };
        }

        /// <summary>
        /// Calculates a typing delay in MS, to make the bot responses appear more natural
        /// </summary>
        /// <param name="textToType">The string that the bot is typing</param>
        /// <param name="charactersPerMinute">typing speed in characters per minute</param>
        /// <param name="thinkingTimeDelay">millisecond delay to wait before starting a response</param>
        /// <returns>A delay in milliseconds to wait while the bot is 'typing' the response</returns>
        public static int CalculateTypingTime(string textToType, int charactersPerMinute, int thinkingTimeDelay)
        {
            return string.IsNullOrEmpty(textToType) ? 0 : thinkingTimeDelay + (textToType.Length * (60 / charactersPerMinute));
        }

        /// <summary>
        /// Creates a pair of choices to represent binary yes/no options
        /// </summary>
        /// <returns>a list of <see cref="Choice"/></returns>
        /// TODO: add text copy to resx
        private static List<Choice> BuildConfirmationChoices()
        {
            return new List<Choice>()
                       {
                           new Choice
                               {
                                   Action = new CardAction(
                                       text: "yes",
                                       title: "yes",
                                       value: "yes"),
                                   Value = "yes",
                                   Synonyms = new List<string>() { "yep", "yeah", "ok", "y" }
                               },
                           new Choice
                               {
                                   Action = new CardAction(
                                       text: "no",
                                       title: "no",
                                       value: "no"),
                                   Value = "no",
                                   Synonyms =
                                       new List<string>() { "nope", "nah", "negative", "n" }
                               },
                       };
        }
    }
}