using System.Collections.Generic;

namespace ESFA.ProvideFeedback.ApprenticeBot.Services
{
    public class ConversationEndOption : IDialogStep
    {
        public string DialogTarget { get; set; }
        public List<string> Responses { get; set; }
    }
}