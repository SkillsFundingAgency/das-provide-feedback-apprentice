namespace ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Conversation
{
    using System;

    [Serializable]
    public class BotSession
    {
        public string ConversationId { get; set; }

        public string SessionId { get; set; }

        public DateTime SessionCreated { get; set; }
    }
}