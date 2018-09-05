using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;

namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Dialogs.Components
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs;
    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Helpers;
    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models;

    using Microsoft.Bot.Builder.Core.Extensions;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Prompts;
    using Microsoft.Bot.Builder.Prompts.Choices;
    using Microsoft.Bot.Schema;
    using Microsoft.Recognizers.Text;

    using ChoicePrompt = ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.ChoicePrompt;
    using ChoicePromptOptions = ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.ChoicePromptOptions;

    public sealed class SurveyQuestionDialog : DialogContainer
    {
        private readonly PromptConfiguration configuration = new PromptConfiguration();

        public SurveyQuestionDialog(string id)
            : base(id)
        {
        }

        public string PromptText { get; private set; }

        public ICollection<IResponse> Responses { get; private set; } = new List<IResponse>();

        public int Score { get; private set; } = 1;

        public SurveyQuestionDialog Build()
        {
            var steps = new WaterfallStep[]
                            {
                                async (dc, args, next) => { await this.Question(dc, args, next); },
                                async (dc, args, next) => { await this.Response(dc, args, next); },
                                async (dc, args, next) => { await dc.End(); }
                            };

            this.Dialogs.Add(this.DialogId, steps);

            // Define the prompts used in this conversation flow.
            this.Dialogs.Add($"{this.DialogId}-prompt", new ChoicePrompt(Culture.English) { Style = ListStyle.None });

            return this;
        }

        public SurveyQuestionDialog WithPrompt(string prompt)
        {
            this.PromptText = prompt;
            return this;
        }

        public SurveyQuestionDialog WithResponse(IResponse response)
        {
            this.Responses.Add(response);
            return this;
        }

        public SurveyQuestionDialog WithResponses(ICollection<IResponse> responses)
        {
            this.Responses = responses;
            return this;
        }

        public SurveyQuestionDialog WithScore(int score)
        {
            this.Score = score;
            return this;
        }

        private async Task Question(DialogContext dc, IDictionary<string, object> args, SkipStepFunction next)
        {
            await dc.Context.SendTypingActivity(this.PromptText);
            await dc.Prompt($"{this.DialogId}-prompt", this.PromptText, PromptConfiguration.Options);
        }

        private async Task Response(DialogContext dc, IDictionary<string, object> args, SkipStepFunction next)
        {
            UserInfo userInfo = UserState<UserInfo>.Get(dc.Context);

            userInfo.SurveyState.Progress = ProgressState.InProgress;

            BinaryQuestionResponse response = await this.ParseResponse(dc, args, this.PromptText, this.Score);

            userInfo.SurveyState.Responses.Add(response);

            foreach (IResponse r in this.Responses)
            {
                if (r is ConditionalResponse<BinaryQuestionResponse> conditionalResponse)
                {
                    if (!conditionalResponse.IsValid(response))
                    {
                        continue;
                    }
                }

                await dc.Context.SendTypingActivity(r.Prompt);
                await dc.Context.SendActivity(r.Prompt);
            }

            // Ask next question
            await next();
        }

        /// <summary>
        /// TODO: pull this out into a configuration object injected at bot start
        /// </summary>
        internal class PromptConfiguration
        {

            public static ChoicePromptOptions Options => new ChoicePromptOptions()
            {
                Attempts = 3,
                TooManyAttemptsString = "Oops, too many incorrect attempts!",
                Choices = Choices,
                RetryPromptString = RetryPromptString,
            };

            private static Choice NegativeChoice => new Choice
            {
                Value = "no",
                Synonyms = new List<string>() { "false", "nope", "nah", "negative", "n" }
            };

            private static Choice PositiveChoice => new Choice
            {
                Value = "yes",
                Synonyms = new List<string>() { "true", "yep", "yeah", "ok", "y" }
            };

            private static List<Choice> Choices => new List<Choice> { PositiveChoice, NegativeChoice };

            private static Activity RetryPromptActivity => MessageFactory.Text(
                RetryPromptString,
                inputHint: InputHints.ExpectingInput);

            private static readonly string RetryPromptString = $"Sorry, I'm just a simple bot. Please type ‘Yes’ or ‘No’";
        }

        private async Task<BinaryQuestionResponse> ParseResponse(DialogContext dc, IDictionary<string, object> args, string questionText, int score = 0)
        {
            return await Task.Run(
                () =>
                {
                    string utterance = dc.Context.Activity.Text;                // What did they say?
                    string intent = (args["Value"] as FoundChoice)?.Value;      // What did they mean?
                    bool positive = intent == "yes";                            // Was it positive?

                    BinaryQuestionResponse feedbackResponse =
                        new BinaryQuestionResponse
                        {
                            Question = questionText,
                            Answer = utterance,
                            Intent = intent,
                            Score = positive ? score : -score
                        };

                    return feedbackResponse;
                });
        }
    }


}