namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Feedback.Components
{
    using System;
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

    public sealed class FreeTextDialog : ComponentDialog, ICustomComponent<FreeTextDialog>
    {
        public const string FreeTextPrompt = "freeTextPrompt";

        private readonly BotSettings botSettings;

        private readonly FeatureToggles features;

        private readonly IFeedbackBotStateRepository state;

        /// <inheritdoc />
        public FreeTextDialog(
            string dialogId,
            IFeedbackBotStateRepository state,
            BotSettings botSettings,
            FeatureToggles features)
            : base(dialogId)
        {
            this.InitialDialogId = dialogId;

            this.botSettings = botSettings;
            this.features = features;
            this.state = state;
        }

        public int PointsAvailable { get; private set; } = 1;

        public string PromptText { get; set; }

        public ICollection<IBotResponse> Responses { get; private set; } = new List<IBotResponse>();

        public FreeTextDialog AddResponse(IBotResponse botResponse)
        {
            this.Responses.Add(botResponse);
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

        public FreeTextDialog WithResponses(ICollection<IBotResponse> responses)
        {
            this.Responses = responses;
            return this;
        }

        public FreeTextDialog WithScore(int score)
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

            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text(this.PromptText) };

            return await stepContext.PromptAsync(FreeTextPrompt, promptOptions, cancellationToken);
        }

        // TODO: Add LUIS for natural language processing
        private FreeTextResponse HandleResponse(DialogContext stepContext)
        {
            string utterance = stepContext.Context.Activity.Text; // What did they say?
            // TODO: add LUIS service to process free text responses
            // string intent = Luis.GetIntention(); // What did they mean?

            bool skipped = utterance.Equals("skip", StringComparison.OrdinalIgnoreCase);

            var feedbackResponse = new FreeTextResponse
            {
                Question = this.PromptText,
                Answer = utterance,
                Score = !skipped ? this.PointsAvailable : 0,
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

            await this.Responses.Create(
                stepContext.Context,
                userProfile.SurveyState,
                this.botSettings,
                this.features,
                cancellationToken);

            // Ask next question
            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }
    }
}