using System.Collections.Generic;

namespace ESFA.ProvideFeedback.ApprenticeBot
{
    /// <summary>
    /// Stores the current conversation state
    /// </summary>
    public class ConvoState
    {
        public ConvoState()
        {
            Messages = new List<string>();
        }

        public List<string> Messages { get; set; }
    }
}