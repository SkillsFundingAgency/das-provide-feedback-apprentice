using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace ESFA.ProvideFeedback.Apprentice.Bot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog
    {
        public Task DialogBegin(DialogContext dc, IDictionary<string, object> dialogArgs = null)
        {
            return Task.CompletedTask;
        }
    }
}
