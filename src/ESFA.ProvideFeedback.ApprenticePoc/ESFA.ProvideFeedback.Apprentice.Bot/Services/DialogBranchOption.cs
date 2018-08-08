using System.Collections.Generic;
using System.Configuration;
using NLog.Fluent;

namespace ESFA.ProvideFeedback.Apprentice.Bot.Services
{
    public class DialogBranchOption : IDialogStep
    {
        public List<string> Responses { get; set; }
        public string DialogTarget { get; set; }
    }
}