﻿namespace ESFA.DAS.ProvideFeedback.Apprentice.Core.State
{
    using System;

    /// <summary>
    /// User state information.
    /// </summary>
    [Serializable]
    public class UserProfile
    {
        public Guid Id { get; set; }

        public string UserId { get; set; }

        public string IlrNumber { get; set; }

        public int? StandardCode { get; set; }

        public DateTime? ApprenticeshipStartDate { get; set; }

        public SurveyState SurveyState { get; set; } = new SurveyState();
    }
}