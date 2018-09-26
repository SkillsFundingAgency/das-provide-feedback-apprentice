using System;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Apprentice.BotV4;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;
using Microsoft.Bot.Builder.Dialogs;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Commands
{
    public sealed class ApprenticeFeedbackSecretTrigger : AdminCommand, IBotDialogCommand
    {
        private readonly FeedbackBotAccessors _accessors;

        public ApprenticeFeedbackSecretTrigger(FeedbackBotAccessors accessors) 
            : base("I like avocado")
        {
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
        }

        public override async Task<DialogTurnResult> ExecuteAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            var dialogId = "afb-v3";

            UserInfo userInfo = await _accessors.UserProfile.GetAsync(dc.Context, () => new UserInfo(), cancellationToken);
            userInfo.SurveyState = new SurveyState
            {
                SurveyId = dialogId, StartDate = DateTime.Now, Progress = ProgressState.InProgress
            };


            return await dc.BeginDialogAsync(dialogId, null, cancellationToken);
        }
    }
}