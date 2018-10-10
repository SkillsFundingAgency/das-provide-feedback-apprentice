namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Feedback.Components
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Conversation;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;

    using Microsoft.Bot.Builder.Dialogs;

    using BotSettings = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Bot;
    using FeatureToggles = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Features;

    /// <inheritdoc />
    public sealed class SurveyEndDialog : ComponentDialog
    {
        private readonly BotSettings botSettings;

        private readonly FeatureToggles features;

        private readonly FeedbackBotStateRepository state;

        private readonly DialogConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="SurveyEndDialog"/> class.
        /// A dialog that appears at the end of a survey. Usually collects the responses and feeds back to the user in some way.
        /// </summary>
        /// <param name="dialogId">the unique id of this dialog.</param>
        /// <param name="state">the state repository/accessors.</param>
        /// <param name="botSettings">configuration for the bot.</param>
        /// <param name="features">feature configuration.</param>
        public SurveyEndDialog(string dialogId, FeedbackBotStateRepository state, BotSettings botSettings, FeatureToggles features)
            : base(dialogId)
        {
            this.botSettings = botSettings;
            this.features = features;
            this.state = state;
            this.configuration = new DialogConfiguration(); // TODO: Inject from IOptions
        }

        /// <summary>
        /// Gets or sets a list of responses to echo back to the user.
        /// </summary>
        public ICollection<IResponse> Responses { get; set; } = new List<IResponse>();

        /// <summary>
        /// Fluent interface for adding a bot response to this dialog. These responses can be conditional, <see cref="ConditionalResponse"/>.
        /// </summary>
        /// <param name="response">the response to add.</param>
        /// <returns>this dialog.</returns>
        public SurveyEndDialog AddResponse(IResponse response)
        {
            this.Responses.Add(response);
            return this;
        }

        /// <summary>
        /// Fluent interface for adding a collection of bot response to this dialog. These responses can be conditional, <see cref="ConditionalResponse"/>.
        /// </summary>
        /// <param name="responses">the responses to add.</param>
        /// <returns>this dialog.</returns>
        public SurveyEndDialog WithResponses(ICollection<IResponse> responses)
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

            if (this.configuration.CollateResponses)
            {
                await this.Responses.RespondAsSingleMessageAsync(stepContext.Context, userProfile.SurveyState, this.configuration, cancellationToken);
            }
            else
            {
                await this.Responses.RespondAsMultipleMessagesAsync(stepContext.Context, userProfile.SurveyState, this.configuration, cancellationToken);
            }

            userProfile.SurveyState.Progress = ProgressState.Complete;
            userProfile.SurveyState.EndDate = DateTime.Now;

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> EndDialogAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(stepContext, cancellationToken);
        }
    }
}