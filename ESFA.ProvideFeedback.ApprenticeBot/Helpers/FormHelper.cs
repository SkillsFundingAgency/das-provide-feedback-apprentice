using System.Collections.Generic;
using ESFA.ProvideFeedback.ApprenticeBot.Services;
using Microsoft.AspNetCore.Hosting.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Prompts.Choices;
using Microsoft.Bot.Schema;
using Microsoft.CodeAnalysis.Text;

namespace ESFA.ProvideFeedback.ApprenticeBot
{
    public static class FormHelper
    {
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
    }
}