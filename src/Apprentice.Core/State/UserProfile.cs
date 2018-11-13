namespace ESFA.DAS.ProvideFeedback.Apprentice.Core.State
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Conversation;

    /// <summary>
    /// User state information.
    /// </summary>
    [Serializable]
    public class UserProfile
    {
        public SurveyState SurveyState { get; set; } = new SurveyState();

        public string UserId { get; set; }

        public string IlrNumber { get; set; }

        public string StandardOrProgram { get; set; }
    }
}