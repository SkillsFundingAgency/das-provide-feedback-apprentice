using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs
{
    public sealed class ChoicePromptOptions : Microsoft.Bot.Builder.Dialogs.PromptOptions
    {
        public long Attempts { get; set; }
        public string TooManyAttemptsString { get; set; }
        public Dictionary<long, string> RetryPromptsCollection { get; set; }
    }
}
