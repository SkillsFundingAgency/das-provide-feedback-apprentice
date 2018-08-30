namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Dialogs;
    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.State;

    using Microsoft.Bot.Builder.Core.Extensions;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Schema;

    public sealed class ResetDialogCommand : AdminCommand
    {

        public ResetDialogCommand()
            : base("reset")
        {
        }

        public override async Task ExecuteAsync(DialogContext dc)
        {
            UserInfo userInfo = UserState<UserInfo>.Get(dc.Context);

            userInfo.SurveyState = new SurveyState();

            await dc.Context.SendActivity($"OK. Resetting conversation...");
            await dc.Replace(RootDialog.Id);

        }
    }
}