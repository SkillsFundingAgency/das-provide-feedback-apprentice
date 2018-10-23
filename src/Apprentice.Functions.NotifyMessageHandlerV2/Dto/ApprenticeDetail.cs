using System;
using ESFA.DAS.ProvideFeedback.Apprentice.Data;
using Newtonsoft.Json;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.Dto
{
    [Serializable]
    public class ApprenticeDetail : TypedDocument<ApprenticeDetail>
    {
        [JsonProperty("unique_learner_number")]
        public string UniqueLearnerNumber { get; set; }

        [JsonProperty("mobile_number")]
        public string MobileNumber { get; set; }

        [JsonProperty("survey_code")]
        public string SurveyCode { get; set; }

        [JsonProperty("sent_date")]
        public DateTime? SentDate { get; set; }
    }

}
