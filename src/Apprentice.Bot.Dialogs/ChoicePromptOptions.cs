namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs
{
    using System.Collections.Generic;

    public sealed class RetryPromptOptions : Microsoft.Bot.Builder.Dialogs.PromptOptions
    {
        public long Attempts { get; set; }

        public Dictionary<long, string> RetryPromptsCollection { get; set; }

        public string TooManyAttemptsString { get; set; }
    }
}