using System.Linq;
using System.Text;

namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Dialogs.Components
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Helpers;
    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;

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
        public SurveyQuestionDialog(string id)
            : base(id)
        {
            Configuration = new DialogConfiguration();
        }

        public DialogConfiguration Configuration { get; }

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

        // TODO: add these strings to CommonStrings.en-GB.resx
        private async Task<BinaryQuestionResponse> ParseResponse(
            DialogContext dc,
            IDictionary<string, object> args,
            string questionText,
            int score = 0)
        {
            return await Task.Run(
                       () =>
                           {
                               string utterance = dc.Context.Activity.Text; // What did they say?
                               string intent = (args["Value"] as FoundChoice)?.Value; // What did they mean?
                               bool positive = intent == "yes"; // Was it positive?

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

        private async Task Question(DialogContext dc, IDictionary<string, object> args, SkipStepFunction next)
        {
            if (Configuration.RealisticTypingDelay)
            {
                await dc.Context.SendTypingActivity(this.PromptText, Configuration.CharactersPerMinute, Configuration.ThinkingTimeDelayMs);
            }
            await dc.Prompt($"{this.DialogId}-prompt", this.PromptText, PromptConfiguration.Options);
        }

        private async Task Response(DialogContext dc, IDictionary<string, object> args, SkipStepFunction next)
        {
            UserInfo userInfo = UserState<UserInfo>.Get(dc.Context);

            userInfo.SurveyState.Progress = ProgressState.InProgress;

            BinaryQuestionResponse response = await this.ParseResponse(dc, args, this.PromptText, this.Score);

            userInfo.SurveyState.Responses.Add(response);

            if (Configuration.CollateResponses)
            {
                await RespondAsSingleMessage(this.Responses, dc, response, userInfo);
            }

            else
            {
                await RespondAsMultipleMessages(this.Responses, dc, response, userInfo);
            }

            // Ask next question
            await next();
        }

        private async Task RespondAsMultipleMessages(IEnumerable<IResponse> responses, DialogContext dc, BinaryQuestionResponse response, UserInfo userInfo)
        {
            foreach (IResponse r in responses)
            {
                if (r is ConditionalResponse<BinaryQuestionResponse> conditionalResponse && !conditionalResponse.IsValid(response))
                {
                    continue;
                }
                if (r is PredicateResponse predicatedResponse && !predicatedResponse.IsValid(userInfo))
                {
                    continue;
                }

                if (Configuration.RealisticTypingDelay)
                {
                    await dc.Context.SendTypingActivity(r.Prompt, Configuration.CharactersPerMinute, Configuration.ThinkingTimeDelayMs);
                }

                await dc.Context.SendActivity(r.Prompt, InputHints.IgnoringInput);
            }
        }

        private async Task RespondAsSingleMessage(IEnumerable<IResponse> responses, DialogContext dc, BinaryQuestionResponse response, UserInfo userInfo)
        {
            StringBuilder sb = new StringBuilder();
            foreach (IResponse r in responses)
            {
                if (r is ConditionalResponse<BinaryQuestionResponse> conditionalResponse && !conditionalResponse.IsValid(response))
                {
                    continue;
                }
                if (r is PredicateResponse predicatedResponse && !predicatedResponse.IsValid(userInfo))
                {
                    continue;
                }

                sb.AppendLine(r.Prompt);
            }

            var reply = sb.ToString();

            if (Configuration.RealisticTypingDelay)
            {
                await dc.Context.SendTypingActivity(reply, Configuration.CharactersPerMinute, Configuration.ThinkingTimeDelayMs);
            }

            await dc.Context.SendActivity(reply, InputHints.IgnoringInput);
        }

        /// <summary>
        /// TODO: pull this out into a configuration object injected at bot start
        /// </summary>
        internal class PromptConfiguration
        {
            // TODO: add these strings to CommonStrings.en-GB.resx
            private static readonly string RetryPromptString =
                $"Sorry, I didn't catch that. Please type 'Yes' or 'No'";

            // TODO: add these strings to CommonStrings.en-GB.resx
            public static ChoicePromptOptions Options =>
                new ChoicePromptOptions()
                    {
                        Attempts = 3,
                        TooManyAttemptsString =
                            "Sorry, I couldn't understand you this time. You'll get another chance to leave feedback in about 3 months. Thanks and goodbye! ",
                        Choices = Choices,
                        RetryPromptString = RetryPromptString,
                        RetryPromptsCollection =
                            new Dictionary<long, string>()
                                {
                                    {
                                        1,
                                        RetryPromptString
                                    },
                                    {
                                        2,
                                        "Please could you answer 'Yes' or 'No'"
                                    }
                                }
                    };

            private static List<Choice> Choices => new List<Choice> { PositiveChoice, NegativeChoice };

            // TODO: add these strings to CommonStrings.en-GB.resx
            private static Choice NegativeChoice =>
                new Choice { Value = "no", Synonyms = new List<string>() { "false", "nope", "nah", "negative", "n" } };

            // TODO: add these strings to CommonStrings.en-GB.resx
            private static Choice PositiveChoice =>
                new Choice { Value = "yes", Synonyms = new List<string>() { "true", "yep", "yeah", "ok", "y" } };

            private static Activity RetryPromptActivity =>
                MessageFactory.Text(RetryPromptString, inputHint: InputHints.ExpectingInput);
        }
    }
}