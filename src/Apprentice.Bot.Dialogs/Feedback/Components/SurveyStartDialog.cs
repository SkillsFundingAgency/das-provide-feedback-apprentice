namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Feedback.Components
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;

    using Microsoft.Bot.Builder.Dialogs;

    using BotSettings = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Bot;
    using FeatureToggles = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Features;

    public sealed class SurveyStartDialog : ComponentDialog
    {
        private readonly BotSettings botSettings;

        private readonly FeatureToggles features;

        private FeedbackBotStateRepository state;

        private DialogConfiguration configuration;

        /// <inheritdoc />
        public SurveyStartDialog(
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

        public ICollection<IResponse> Responses { get; private set; } = new List<IResponse>();

        public SurveyStartDialog AddResponse(IResponse response)
        {
            this.Responses.Add(response);
            return this;
        }

        public SurveyStartDialog Build()
        {
            var steps = new WaterfallStep[]
            {
                this.StepAsync,
                this.FinishDialogAsync,
            };

            var waterfall = new WaterfallDialog(this.Id, steps);

            this.AddDialog(waterfall);
            this.AddDialog(new ConfirmPrompt("confirm"));

            return this;
        }

        public SurveyStartDialog WithResponses(ICollection<IResponse> responses)
        {
            this.Responses = responses;
            return this;
        }

        private async Task<DialogTurnResult> StepAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var userInfo = await this.state.UserProfile.GetAsync(
                               stepContext.Context,
                               () => new UserProfile(),
                               cancellationToken);

            if (this.configuration.CollateResponses)
            {
                await this.Responses.RespondAsSingleMessageAsync(
                    stepContext.Context,
                    userInfo.SurveyState,
                    this.configuration,
                    cancellationToken);
            }
            else
            {
                await this.Responses.RespondAsMultipleMessagesAsync(
                    stepContext.Context,
                    userInfo.SurveyState,
                    this.configuration,
                    cancellationToken);
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        public async Task<DialogTurnResult> FinishDialogAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken) =>
            await stepContext.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}