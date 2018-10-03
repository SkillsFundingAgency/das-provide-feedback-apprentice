namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Dialogs.Components
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors;
    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Helpers;
    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;

    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Choices;
    using Microsoft.Bot.Schema;

    using AzureSettings = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Azure;
    using BotSettings = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Bot;
    using DataSettings = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Data;
    using FeatureToggles = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Features;
    using NotifySettings = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Notify;
    using PromptOptions = ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.RetryPromptOptions;

    public sealed class SurveyQuestionDialog : ComponentDialog
    {
        public const string ChoicePrompt = "choicePrompt";

        private BotSettings botSettings;

        private DialogConfiguration configuration;

        private FeatureToggles features;

        private FeedbackBotStateRepository state;

        /// <inheritdoc />
        public SurveyQuestionDialog(
            string dialogId,
            FeedbackBotStateRepository state,
            BotSettings botSettings,
            FeatureToggles features)
            : base(dialogId)
        {
            this.botSettings = botSettings;
            this.features = features;
            this.state = state;
            this.configuration = new DialogConfiguration(); // TODO: Inject from IOptions
        }

        public int PointsAvailable { get; private set; } = 1;

        public string PromptText { get; private set; }

        public ICollection<IResponse> Responses { get; private set; } = new List<IResponse>();

        public SurveyQuestionDialog Build()
        {
            var steps = new WaterfallStep[] { this.AskQuestionAsync, this.RespondToAnswerAsync, this.EndDialogAsync, };

            var waterfall = new WaterfallDialog(this.Id, steps);

            this.AddDialog(waterfall);

            // this.AddDialog(new ChoicePrompt($"{this.Id}-prompt", defaultLocale: Culture.English) { Style = ListStyle.None });
            this.AddDialog(new ChoicePrompt(ChoicePrompt));

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
            this.PointsAvailable = score;
            return this;
        }

        private async Task<DialogTurnResult> AskQuestionAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            if (this.configuration.RealisticTypingDelay)
            {
                await stepContext.Context.SendTypingActivityAsync(
                    this.PromptText,
                    this.configuration.CharactersPerMinute,
                    this.configuration.ThinkingTimeDelayMs);
            }

            var promptOptions = PromptConfiguration.RetryPromptOptions;
            promptOptions.Prompt = MessageFactory.Text(this.PromptText);

            return await stepContext.PromptAsync(ChoicePrompt, promptOptions, cancellationToken);
        }

        private BinaryQuestionResponse CreateResponse(WaterfallStepContext stepContext)
        {
            string utterance = stepContext.Context.Activity.Text; // What did they say?
            string intent = (stepContext.Result as FoundChoice)?.Value; // What did they mean?

            bool positive = intent == "yes"; // Was it positive?

            BinaryQuestionResponse feedbackResponse = new BinaryQuestionResponse
                {
                    Question = this.PromptText,
                    Answer = utterance,
                    Intent = intent,
                    Score = positive ? this.PointsAvailable : -this.PointsAvailable,
                };

            return feedbackResponse;
        }

        private async Task<DialogTurnResult> RespondToAnswerAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            UserProfile userProfile = await this.state.UserProfile.GetAsync(
                                          stepContext.Context,
                                          () => new UserProfile(),
                                          cancellationToken);

            userProfile.SurveyState.Progress = ProgressState.InProgress;

            BinaryQuestionResponse feedbackResponse = this.CreateResponse(stepContext);

            userProfile.SurveyState.Responses.Add(feedbackResponse);

            if (this.configuration.CollateResponses)
            {
                await this.Responses.RespondAsSingleMessageAsync(
                    stepContext.Context,
                    userProfile.SurveyState,
                    this.configuration,
                    cancellationToken);
            }
            else
            {
                await this.Responses.RespondAsMultipleMessagesAsync(
                    stepContext.Context,
                    userProfile.SurveyState,
                    this.configuration,
                    cancellationToken);
            }

            // Ask next question
            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> EndDialogAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        /// <summary>
        /// TODO: pull this out into a configuration object injected at bot start </summary>
        internal class PromptConfiguration
        {
            // TODO: add these strings to CommonStrings.en-GB.resx
            private static readonly string RetryPromptString = $"Sorry, I didn't catch that. Please type 'Yes' or 'No'";

            // TODO: add these strings to CommonStrings.en-GB.resx
            public static PromptOptions RetryPromptOptions
            {
                get
                {
                    var options = new PromptOptions()
                        {
                            Attempts = 3,
                            TooManyAttemptsString =
                                "Sorry, I couldn't understand you this time. You'll get another chance to leave feedback in about 3 months. Thanks and goodbye! ",
                            Choices = Choices,
                            RetryPrompt = new Activity(type: "message", text: RetryPromptString),
                            RetryPromptsCollection = new Dictionary<long, string>
                                {
                                    { 1, RetryPromptString }, { 2, "Please could you answer 'Yes' or 'No'" },
                                },
                        };

                    return options;
                }
            }

            private static List<Choice> Choices => new List<Choice> { PositiveChoice, NegativeChoice };

            // TODO: add these strings to CommonStrings.en-GB.resx
            private static Choice NegativeChoice =>
                new Choice
                    {
                        Value = "no",
                        Synonyms = new List<string>()
                        {
                            "false",
                            "nope",
                            "nah",
                            "negative",
                            "n",
                        },
                    };

            // TODO: add these strings to CommonStrings.en-GB.resx
            private static Choice PositiveChoice =>
                new Choice
                    {
                        Value = "yes",
                        Synonyms = new List<string>()
                        {
                            "aye",
                            "true",
                            "yep",
                            "yeah",
                            "ok",
                            "y",
                        },
                    };

            private static Activity RetryPromptActivity =>
                MessageFactory.Text(RetryPromptString, inputHint: InputHints.ExpectingInput);
        }
    }
}