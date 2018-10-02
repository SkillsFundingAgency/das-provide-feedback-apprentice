namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Helpers
{
    using System.Collections.Generic;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs;

    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Choices;
    using Microsoft.Bot.Schema;

    public static class FormHelper
    {
        /// <summary>
        /// Creates an options list for binary yes/no confirmation prompts.
        /// </summary>
        public static ChoicePromptOptions PolarQuestionOptions =>
            new ChoicePromptOptions()
                {
                    Choices = BuildConfirmationChoices(),
                    RetryPrompt = MessageFactory.Text(
                        "Sorry, I'm just a simple bot. Please type ‘Yes’ or ‘No’",
                        inputHint: InputHints.ExpectingInput),
                };

        /// <summary>
        /// Calculates a typing delay in MS, to make the bot responses appear more natural
        /// </summary>
        /// <param name="textToType">The string that the bot is typing</param>
        /// <param name="charactersPerMinute">typing speed in characters per minute</param>
        /// <param name="thinkingTimeDelay">millisecond delay to wait before starting a response</param>
        /// <returns>A delay in milliseconds to wait while the bot is 'typing' the response</returns>
        public static int CalculateTypingTime(string textToType, int charactersPerMinute, int thinkingTimeDelay)
        {
            if (string.IsNullOrEmpty(textToType))
            {
                return 0;
            }

            var charactersPerSecond = charactersPerMinute / 60;
            var typingDelay = textToType.Length / charactersPerSecond * 1000;
            return thinkingTimeDelay + typingDelay;
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
                                   Synonyms = new List<string>()
                                                  {
                                                      "true",
                                                      "yep",
                                                      "yeah",
                                                      "ok",
                                                      "y"
                                                  }
                               },
                           new Choice
                               {
                                   Action = new CardAction(text: "no", title: "no", value: "no"),
                                   Value = "no",
                                   Synonyms = new List<string>()
                                                  {
                                                      "false",
                                                      "nope",
                                                      "nah",
                                                      "negative",
                                                      "n"
                                                  }
                               },
                       };
        }
    }
}