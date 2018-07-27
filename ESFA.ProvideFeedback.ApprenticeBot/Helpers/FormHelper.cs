using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Prompts.Choices;
using Microsoft.Bot.Schema;

namespace ESFA.ProvideFeedback.ApprenticeBot
{
    public static class FormHelper
    {
        public static ChoicePromptOptions ConfirmationPromptOptions
        {
            get
            {
                return new ChoicePromptOptions()
                {
                    Choices = BuildConfirmationChoices(),
                    RetryPromptActivity = MessageFactory.Text("Please reply YES or NO") as Activity,
                };
            }
        }

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
                }
                //new Choice {Action = new CardAction(text: "stop", title: "stop", value: "stop"), Value = "stop"}
            };
        }
    }
}