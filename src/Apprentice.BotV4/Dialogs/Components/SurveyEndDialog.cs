namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Dialogs.Components
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Helpers;
    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.State;

    using Microsoft.Bot.Builder.Core.Extensions;
    using Microsoft.Bot.Builder.Dialogs;

    public sealed class SurveyEndDialog : SingleStepDialog
    {
        public SurveyEndDialog(string id)
            : base(id)
        {
        }

        protected override async Task Step(DialogContext dc, IDictionary<string, object> args, SkipStepFunction next)
        {
            UserInfo userInfo = UserState<UserInfo>.Get(dc.Context);

            foreach (IResponse r in this.Responses)
            {
                if (r is PredicateResponse predicatedResponse)
                {
                    if (!predicatedResponse.IsValid(userInfo))
                    {
                        continue;
                    }
                }

                await dc.Context.SendTypingActivity(r.Prompt);
                await dc.Context.SendActivity(r.Prompt);
            }

            userInfo.SurveyState.Progress = ProgressState.Complete;
            userInfo.SurveyState.EndDate = DateTime.Now;

            await dc.End();
        }
    }
}