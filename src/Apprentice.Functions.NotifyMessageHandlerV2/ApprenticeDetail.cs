using System;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2
{
    public class ApprenticeDetail
    {
        public string PhoneNumber { get; internal set; }
        public string Ulr { get; internal set; }
        public string UniqueSurveyCode { get; internal set; }
        public DateTime? SurveySentDate { get; internal set; }
    }
}