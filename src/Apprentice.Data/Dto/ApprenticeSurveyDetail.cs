using System;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Data.Dto
{
    public class ApprenticeSurveyInvite
    {
        public string UniqueLearnerNumber { get; set; }

        public long MobileNumber { get; set; }

        public long Ukprn { get; set; }

        public string SurveyCode { get; set; }

        public DateTime? SentDate { get; set; }
    }
}
