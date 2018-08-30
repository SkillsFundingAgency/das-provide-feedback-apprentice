namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.State
{
    using System.Collections.Generic;

    /// <summary>
    /// Conversation state information.
    /// We are also using this directly for dialog state, which needs an <see cref="IDictionary{string, object}"/> object.
    /// </summary>
    public class ConversationInfo : Dictionary<string, object>
    {
        private const string BadResponsesKey = "BadResponses";

        public ConversationInfo()
        {
            this[BadResponsesKey] = 0;
        }

        public long BadResponses
        {
            get => (long)this[BadResponsesKey];
            set => this[BadResponsesKey] = value;
        }
    }
}