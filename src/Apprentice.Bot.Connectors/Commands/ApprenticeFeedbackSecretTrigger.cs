﻿using System;
using System.Threading;
using System.Threading.Tasks;

using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;
using Microsoft.Bot.Builder.Dialogs;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Commands
{
    public sealed class ApprenticeFeedbackSecretTrigger : AdminCommand, IBotDialogCommand
    {
        private readonly FeedbackBotState state;

        public ApprenticeFeedbackSecretTrigger(FeedbackBotState state) 
            : base("I like avocado")
        {
            this.state = state ?? throw new ArgumentNullException(nameof(state));
        }

        public override async Task<DialogTurnResult> ExecuteAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            var dialogId = "afb-v3";

            UserInfo userInfo = await this.state.UserInfo.GetAsync(dc.Context, () => new UserInfo(), cancellationToken);
            userInfo.SurveyState = new SurveyState
            {
                SurveyId = dialogId, StartDate = DateTime.Now, Progress = ProgressState.InProgress
            };


            return await dc.BeginDialogAsync(dialogId, null, cancellationToken);
        }
    }
}