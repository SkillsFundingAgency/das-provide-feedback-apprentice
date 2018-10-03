namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Commands
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;

    using Microsoft.Azure.ServiceBus;
    using Microsoft.Bot.Builder.Dialogs;

    using Newtonsoft.Json.Serialization;

    public sealed class ApprenticeFeedbackSecretTrigger : AdminCommand, IBotDialogCommand
    {
        private readonly FeedbackBotStateRepository state;

        public ApprenticeFeedbackSecretTrigger(FeedbackBotStateRepository state) : base("I like avocado")
        {
            this.state = state;
        }

        /// <inheritdoc />
        public override async Task<DialogTurnResult> ExecuteAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            var dialogId = "afb-v3";

            UserProfile userProfile = await this.state.UserProfile.GetAsync(dc.Context, () => new UserProfile(), cancellationToken);
            userProfile.SurveyState = new SurveyState
                                       {
                                           SurveyId = dialogId,
                                           StartDate = DateTime.Now,
                                           Progress = ProgressState.InProgress
                                       };

            return await dc.BeginDialogAsync(dialogId, cancellationToken: cancellationToken);
        }
    }
}