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
        public string UserId { get; set; }

        public string IlrNumber { get; set; }

        public int? StandardCode { get; set; }

        public DateTime? ApprenticeshipStartDate { get; set; }

        public SurveyState SurveyState { get; set; } = new SurveyState();
    }
}