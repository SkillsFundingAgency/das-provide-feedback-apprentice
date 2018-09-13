using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs
{
    public sealed class ChoicePromptOptions : Microsoft.Bot.Builder.Dialogs.ChoicePromptOptions
    {
        public long Attempts
        {
            get => this.GetProperty<long>(nameof(Attempts));
            set => this[nameof(Attempts)] = (object)value;
        }

        public string TooManyAttemptsString
        {
            get => this.GetProperty<string>(nameof(TooManyAttemptsString));
            set => this[nameof(TooManyAttemptsString)] = (object)value;
        }

        public Dictionary<long, string> RetryPromptsCollection
        {
            get => this.GetProperty<Dictionary<long, string>>(nameof(this.RetryPromptsCollection));
            set => this[nameof(this.RetryPromptsCollection)] = (object)value;
        }
    }
}
