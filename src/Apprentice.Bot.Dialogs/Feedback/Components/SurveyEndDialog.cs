namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Feedback.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Conversation;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Feedback;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;
    using ESFA.DAS.ProvideFeedback.Apprentice.Services;

    using Microsoft.Bot.Builder.Dialogs;
    using BotSettings = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Bot;
    using FeatureToggles = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Features;

    /// <inheritdoc />
    public sealed class SurveyEndDialog : ComponentDialog, ICustomComponent<SurveyEndDialog>
    {
        private readonly BotSettings botSettings;

        private readonly FeatureToggles features;

        private readonly FeedbackBotStateRepository state;

        private readonly IFeedbackService feedbackService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SurveyEndDialog"/> class.
        /// A dialog that appears at the end of a survey. Usually collects the responses and feeds back to the user in some way.
        /// </summary>
        /// <param name="dialogId">the unique id of this dialog.</param>
        /// <param name="state">the state repository/accessors.</param>
        /// <param name="botSettings">configuration for the bot.</param>
        /// <param name="features">feature configuration.</param>
        /// <param name="feedbackService">the service used for storing feedback</param>
        public SurveyEndDialog(
            string dialogId, 
            FeedbackBotStateRepository state, 
            BotSettings botSettings, 
            FeatureToggles features, 
            IFeedbackService feedbackService)
            : base(dialogId)
        {
            this.botSettings = botSettings;
            this.features = features;
            this.feedbackService = feedbackService;
            this.state = state;
        }

        /// <summary>
        /// Gets or sets a list of responses to echo back to the user.
        /// </summary>
        public ICollection<IBotResponse> Responses { get; set; } = new List<IBotResponse>();

        /// <summary>
        /// Fluent interface for adding a bot response to this dialog. These responses can be conditional, <see cref="ConditionalBotResponse"/>.
        /// </summary>
        /// <param name="botResponse">the response to add.</param>
        /// <returns>this dialog.</returns>
        public SurveyEndDialog AddResponse(IBotResponse botResponse)
        {
            this.Responses.Add(botResponse);
            return this;
        }

        /// <summary>
        /// Fluent interface for adding a collection of bot response to this dialog. These responses can be conditional, <see cref="ConditionalBotResponse"/>.
        /// </summary>
        /// <param name="responses">the responses to add.</param>
        /// <returns>this dialog.</returns>
        public SurveyEndDialog WithResponses(ICollection<IBotResponse> responses)
        {
            this.Responses = responses;
            return this;
        }

        /// <summary>
        /// Build the dialog.
        /// </summary>
        /// <returns>A <see cref="SurveyEndDialog"/>.</returns>
        public SurveyEndDialog Build()
        {
            var steps = new WaterfallStep[]
            {
                this.StepAsync,
                this.EndDialogAsync,
            };

            var waterfall = new WaterfallDialog(this.Id, steps);

            this.AddDialog(waterfall);

            return this;
        }

        private async Task<DialogTurnResult> StepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await this.state.UserProfile.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

            await this.Responses.Create(
                stepContext.Context,
                userProfile.SurveyState,
                this.botSettings,
                this.features,
                cancellationToken);

            userProfile.SurveyState.Progress = ProgressState.Complete;
            userProfile.SurveyState.EndDate = DateTime.Now;

            ApprenticeFeedback feedback = CreateFeedbackDto(userProfile);
            await this.feedbackService.SaveFeedbackAsync(feedback);

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Package the FeedbackDTO from the user profile session data
        /// </summary>
        /// <param name="userProfile">the user profile from bot state</param>
        /// <returns>Feedback model</returns>
        private static ApprenticeFeedback CreateFeedbackDto(UserProfile userProfile) =>
            new ApprenticeFeedback
            {
                Apprentice = new Apprentice
                {

                    UniqueLearnerNumber = userProfile.IlrNumber,
                    ApprenticeId = userProfile.UserId,
                },
                Apprenticeship = new Apprenticeship
                {
                    StandardCode = userProfile.StandardCode,
                    ApprenticeshipStartDate = userProfile.ApprenticeshipStartDate
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

        private async Task<DialogTurnResult> EndDialogAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(stepContext, cancellationToken);
        }
    }
}