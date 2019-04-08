namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Feedback.Components
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Helpers;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Conversation;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Feedback;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;
    using ESFA.DAS.ProvideFeedback.Apprentice.Services;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Choices;
    using Microsoft.Bot.Schema;

    using BotSettings = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Bot;
    using FeatureToggles = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Features;
    using PromptOptions = ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Feedback.Components.RetryPromptOptions;

    /// <summary>
    /// A MultipleChoiceDialog, commonly used for survey questions that expect a singular response from a closed set.
    /// </summary>
    public sealed class MultipleChoiceDialog : ComponentDialog, ICustomComponent<MultipleChoiceDialog>
    {
        public const string ChoicePrompt = "choicePrompt";

        private readonly BotSettings botSettings;

        private readonly FeatureToggles features;

        private readonly IFeedbackBotStateRepository state;

        private readonly IFeedbackService feedbackService;

        /// <inheritdoc />
        public MultipleChoiceDialog(
            string dialogId,
            IFeedbackBotStateRepository state,
            BotSettings botSettings,
            FeatureToggles features,
            IFeedbackService feedbackService)
            : base(dialogId)
        {
            this.botSettings = botSettings;
            this.features = features;
            this.state = state;
            this.feedbackService = feedbackService;
        }

        public int PointsAvailable { get; private set; } = 1;

        public string PromptText { get; private set; }

        public ICollection<IBotResponse> Responses { get; private set; } = new List<IBotResponse>();

        public MultipleChoiceDialog Build()
        {
            var steps = new WaterfallStep[]
                {
                    this.AskQuestionAsync,
                    this.SendResponseToUserAsync,
                    this.EndDialogAsync,
                };

            var waterfall = new WaterfallDialog(this.Id, steps);

            this.AddDialog(waterfall);

            this.AddDialog(new ChoicePrompt(ChoicePrompt, state) { Style = ListStyle.None });

            return this;
        }

        public MultipleChoiceDialog WithPrompt(string prompt)
        {
            this.PromptText = prompt;
            return this;
        }

        public MultipleChoiceDialog WithResponse(IBotResponse botResponse)
        {
            this.Responses.Add(botResponse);
            return this;
        }

        public MultipleChoiceDialog WithResponses(ICollection<IBotResponse> responses)
        {
            this.Responses = responses;
            return this;
        }

        public MultipleChoiceDialog WithScore(int score)
        {
            this.PointsAvailable = score;
            return this;
        }

        private async Task<DialogTurnResult> AskQuestionAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            if (this.features.RealisticTypingDelay)
            {
                await stepContext.Context.SendTypingActivityAsync(
                    this.PromptText,
                    this.botSettings.Typing.CharactersPerMinute,
                    this.botSettings.Typing.ThinkingTimeDelay);
            }

            var promptOptions = PromptConfiguration.RetryPromptOptions;
            promptOptions.Prompt = MessageFactory.Text(this.PromptText);

            return await stepContext.PromptAsync(ChoicePrompt, promptOptions, cancellationToken);
        }

        private MultipleChoiceQuestionResponse HandleUserResponse(WaterfallStepContext stepContext)
        {
            string utterance = stepContext.Context.Activity.Text; // What did they say?
            string intent = (stepContext.Result as FoundChoice)?.Value; // What did they mean?

            bool positive = intent == "yes"; // Was it positive?

            var feedbackResponse = new MultipleChoiceQuestionResponse()
            {
                Question = this.PromptText,
                Answer = utterance,
                Intent = intent,
                Score = positive ? this.PointsAvailable : -this.PointsAvailable,
            };

            return feedbackResponse;
        }

        private async Task<DialogTurnResult> SendResponseToUserAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            UserProfile userProfile = await this.state.UserProfile.GetAsync(
                                          stepContext.Context,
                                          () => new UserProfile(),
                                          cancellationToken);

            userProfile.SurveyState.Progress = ProgressState.InProgress;
            MultipleChoiceQuestionResponse feedbackResponse = this.HandleUserResponse(stepContext);

            userProfile.SurveyState.Responses.Add(feedbackResponse);

            ApprenticeFeedback feedback = CreateFeedbackDto(userProfile);
            
            await this.feedbackService.SaveFeedbackAsync(feedback);

            await this.Responses.Create(
                stepContext.Context,
                userProfile.SurveyState,
                this.botSettings,
                this.features,
                cancellationToken);

            // Ask next question
            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private Task<DialogTurnResult> EndDialogAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            return stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Package the FeedbackDTO from the user profile session data
        /// </summary>
        /// <param name="userProfile">the user profile from bot state</param>
        /// <returns>Feedback model</returns>
        private static ApprenticeFeedback CreateFeedbackDto(UserProfile userProfile) =>
            new ApprenticeFeedback
            {
                Id = userProfile.Id,
                Apprentice = new Apprentice
                {

                    UniqueLearnerNumber = userProfile.IlrNumber,
                    ApprenticeId = userProfile.UserId,
                },
                Apprenticeship = new Apprenticeship
                {
                    StandardCode = userProfile.StandardCode.GetValueOrDefault(),
                    ApprenticeshipStartDate = userProfile.ApprenticeshipStartDate.GetValueOrDefault()
                },
                SurveyId = userProfile.SurveyState.SurveyId,
                StartTime = userProfile.SurveyState.StartDate,
                FinishTime = userProfile.SurveyState.EndDate.GetValueOrDefault(),
                Responses = userProfile.SurveyState.Responses.Select(ConvertToResponseData).ToList()
            };

        /// <summary>
        /// Package the responses
        /// </summary>
        /// <param name="questionResponse"></param>
        /// <returns></returns>
        private static ApprenticeResponse ConvertToResponseData(IQuestionResponse questionResponse) =>
            new ApprenticeResponse
            {
                Question = questionResponse.Question,
                Answer = questionResponse.Answer,
                Intent = questionResponse.Intent,
                Score = questionResponse.Score
            };

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
                            "na",
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
                            "yeh",
                            "yea",
                            "yah",
                            "yup",
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