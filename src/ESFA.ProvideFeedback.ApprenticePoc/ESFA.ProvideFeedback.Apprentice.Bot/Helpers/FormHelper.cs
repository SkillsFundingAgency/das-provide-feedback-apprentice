using System.Collections.Generic;
using ESFA.ProvideFeedback.Apprentice.Bot.Services;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Prompts.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace ESFA.ProvideFeedback.Apprentice.Bot.Helpers
{
    public static class FormHelper
    {
        private static IConfiguration Configuration { get; set; }

        public static ChoicePromptOptions ConfirmationPromptOptions => new ChoicePromptOptions()
        {
            Choices = BuildConfirmationChoices(),
            RetryPromptActivity = MessageFactory.Text("Please reply YES or NO") as Activity,
        };

        private static List<Choice> BuildConfirmationChoices()
        {
            return new List<Choice>()
            {
                new Choice
                {
                    Action = new CardAction(text: "yes", title: "yes", value: "yes"),
                    Value = "yes",
                    Synonyms = new List<string>() {"yep", "yeah", "ok", "y"}
                },
                new Choice
                {
                    Action = new CardAction(text: "no", title: "no", value: "no"),
                    Value = "no",
                    Synonyms = new List<string>() {"nope", "nah", "negative", "n"}
                },
                new Choice
                {
                    Action = new CardAction(text: "skip", title: "skip", value: "skip"),
                    Value = "skip",
                    Synonyms = new List<string>() {"next"}
                }
            };
        }

        public static IDialogStep BuildConversationPath(string target)
        {
            return new DialogBranchOption
            {
                DialogTarget = target,
                Responses = new List<string>()
            };
        }

        public static IDialogStep BuildConversationEndOption()
        {
            return new ConversationEndOption
            {
                DialogTarget = null,
                Responses = new List<string>()
            };
        }

        public static int CalculateTypingTimeInMs(string textToType)
        {
            var msPerLetter = 60 / 300;
            var msResponseDelay = 1000;
            return string.IsNullOrEmpty(textToType) ? 0 : msResponseDelay + (textToType.Length * msPerLetter);
        }
    }
}