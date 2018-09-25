using System.Linq;
using System.Text;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;

namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Dialogs.Components
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Helpers;
    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models;

    using Microsoft.Bot.Builder.Core.Extensions;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Schema;

    public sealed class SurveyStartDialog : SingleStepDialog
    {
        public SurveyStartDialog(string id)
            : base(id)
        {
        }

        protected override async Task Step(DialogContext dc, IDictionary<string, object> args, SkipStepFunction next)
        {
            UserInfo userInfo = UserState<UserInfo>.Get(dc.Context);

            if (Configuration.CollateResponses)
            {
                await RespondAsSingleMessage(this.Responses, dc, userInfo);
            }

            else
            {
                await RespondAsMultipleMessages(this.Responses, dc, userInfo);
            }

            await next();
        }
    }
}