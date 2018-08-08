namespace ESFA.ProvideFeedback.Apprentice.Bot.Config
{
    public class DataConfig
    {
        public string DatabaseName { get; set; }

        public string FeedbackTable { get; set; }
        public string ConversationLogTable { get; set; }
        public string SessionStateTable { get; set; }
    }
}