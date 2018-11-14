namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Feedback.Components
{
    public class DialogConfiguration
    {
        public int CharactersPerMinute { get; set; } = 15000;

        public bool CollateResponses { get; set; } = true;

        public bool RealisticTypingDelay { get; set; } = true;

        public int ThinkingTimeDelayMs { get; set; } = 0;
    }
}