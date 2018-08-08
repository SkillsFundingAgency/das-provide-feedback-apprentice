namespace ESFA.ProvideFeedback.Apprentice.Bot.Config
{
    public class BotConfig
    {
        public class TypingConfig
        {
            public int CharactersPerMinute { get; private set; }
            public int ResponseThinkingTime { get; private set; }
        }

        public TypingConfig Typing { get; set; }

        public string UserStateTableName { get; set; }
        public string ConvoStateTableName { get; set; }

    }
}