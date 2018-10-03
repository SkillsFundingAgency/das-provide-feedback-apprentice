using System.Linq;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;

namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Dialogs.Components
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors;
    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Helpers;
    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models;

    using Microsoft.Bot.Builder.Dialogs;

    public sealed class SurveyEndDialog : ComponentDialog
    {
        public SurveyEndDialog(FeedbackBotState state)
            : base("end")
        {
        }


        protected override async Task<DialogTurnResult> StepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserInfo userInfo = await this.State.UserInfo.GetAsync(
                                    stepContext.Context,
                                    cancellationToken: cancellationToken);

            if (this.Configuration.CollateResponses)
            {
                await this.RespondAsSingleMessage(this.Responses, stepContext, userInfo);
            }

            else
            {
                await this.RespondAsMultipleMessages(this.Responses, stepContext, userInfo);
            }

            userInfo.SurveyState.Progress = ProgressState.Complete;
            userInfo.SurveyState.EndDate = DateTime.Now;

            await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
    }
}