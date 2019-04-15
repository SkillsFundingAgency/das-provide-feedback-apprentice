using System;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Data.Dto
{
    public class ApprenticeSurveyInvite
    {
        public Guid Id { get; set; }

        public string UniqueLearnerNumber { get; set; }

        public long MobileNumber { get; set; }

        public long Ukprn { get; set; }

        public string SurveyCode { get; set; }

        public int StandardCode { get; set; }

        public DateTime ApprenticeshipStartDate { get; set; }

        public DateTime? SentDate { get; set; }
    }
}
