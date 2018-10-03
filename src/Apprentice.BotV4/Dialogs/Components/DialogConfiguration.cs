namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Dialogs.Components
{
    public class DialogConfiguration
    {
        public int CharactersPerMinute { get; set; } = 1500;

        public bool CollateResponses { get; set; } = true;

        public bool RealisticTypingDelay { get; set; } = false;

        public int ThinkingTimeDelayMs { get; set; } = 0;
    }
}