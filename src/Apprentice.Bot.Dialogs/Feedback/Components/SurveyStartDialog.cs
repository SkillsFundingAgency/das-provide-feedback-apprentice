namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Feedback.Components
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;

    using Microsoft.Bot.Builder.Dialogs;

    using BotSettings = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Bot;
    using FeatureToggles = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Features;

    public sealed class SurveyStartDialog : ComponentDialog, ICustomComponent<SurveyStartDialog>
    {
        private readonly BotSettings botSettings;

        private readonly FeatureToggles features;

        private readonly FeedbackBotStateRepository state;

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
        }

        public ICollection<IBotResponse> Responses { get; private set; } = new List<IBotResponse>();

        public SurveyStartDialog AddResponse(IBotResponse botResponse)
        {
            this.Responses.Add(botResponse);
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

        public SurveyStartDialog WithResponses(ICollection<IBotResponse> responses)
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

            await this.Responses.Create(
                stepContext.Context,
                userInfo.SurveyState,
                this.botSettings,
                this.features,
                cancellationToken);

            userInfo.Id = Guid.NewGuid();
            userInfo.UserId = stepContext.Context.Activity.From.Id;
            return await stepContext.NextAsync(null, cancellationToken);
        }

        public async Task<DialogTurnResult> FinishDialogAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken) =>
            await stepContext.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}