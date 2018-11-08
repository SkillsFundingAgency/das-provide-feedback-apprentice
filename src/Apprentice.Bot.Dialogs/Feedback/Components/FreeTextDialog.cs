namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Feedback.Components
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Helpers;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Conversation;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;

    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;

    using BotSettings = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Bot;
    using FeatureToggles = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Features;

    public sealed class FreeTextDialog : ComponentDialog, ICustomComponent
    {
        public const string FreeTextPrompt = "freeTextPrompt";

        private readonly BotSettings botSettings;

        private readonly DialogConfiguration configuration;

        private readonly FeatureToggles features;

        private readonly FeedbackBotStateRepository state;

        /// <inheritdoc />
        public FreeTextDialog(
            string dialogId,
            FeedbackBotStateRepository state,
            BotSettings botSettings,
            FeatureToggles features)
            : base(dialogId)
        {
            this.InitialDialogId = dialogId;

            this.botSettings = botSettings;
            this.features = features;
            this.state = state;
            this.configuration = new DialogConfiguration(); // TODO: Inject from IOptions
        }

        public string PromptText { get; set; }

        public ICollection<IResponse> Responses { get; private set; } = new List<IResponse>();

        public FreeTextDialog AddResponse(IResponse response)
        {
            this.Responses.Add(response);
            return this;
        }

        public FreeTextDialog Build()
        {
            var steps = new WaterfallStep[]
            {
                this.AskQuestionAsync,
                this.SendResponseAsync,
                this.FinishDialogAsync,
            };

            var waterfall = new WaterfallDialog(this.Id, steps);

            this.AddDialog(waterfall);

            this.AddDialog(new TextPrompt(FreeTextPrompt));

            return this;
        }

        public async Task<DialogTurnResult> FinishDialogAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken) =>
            await stepContext.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        public FreeTextDialog WithPrompt(string prompt)
        {
            this.PromptText = prompt;
            return this;
        }

        public FreeTextDialog WithResponses(ICollection<IResponse> responses)
        {
            this.Responses = responses;
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

            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text(this.PromptText) };

            return await stepContext.PromptAsync(FreeTextPrompt, promptOptions, cancellationToken);
        }

        // TODO: Add LUIS for natural language processing
        private FreeTextResponse HandleResponse(WaterfallStepContext stepContext)
        {
            string utterance = stepContext.Context.Activity.Text; // What did they say?

            var feedbackResponse = new FreeTextResponse
            {
                Question = this.PromptText,
                Answer = utterance
            };

            return feedbackResponse;
        }

        private async Task<DialogTurnResult> SendResponseAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await this.state.UserProfile.GetAsync(
                stepContext.Context,
                () => new UserProfile(),
                cancellationToken);

            userProfile.SurveyState.Progress = ProgressState.InProgress;

            FreeTextResponse feedbackResponse = this.HandleResponse(stepContext);

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
    }
}