using System;
using ESFA.DAS.ProvideFeedback.Apprentice.Data;
using Newtonsoft.Json;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.Dto
{
    [Serializable]
    public class ApprenticeDetail : TypedDocument<ApprenticeDetail>
    {
        private DateTime? sentDate;

        [JsonProperty("uniqueLearnerNumber")]
        public string UniqueLearnerNumber { get; set; }

        [JsonProperty("mobileNumber")]
        public string MobileNumber { get; set; }

        [JsonProperty("surveyCode")]
        public string SurveyCode { get; set; }

        [JsonProperty("sentDate")]
        public DateTime? SentDate
        {
            get
            {
                return this.sentDate;
            }
            set
            {
                this.SetPropertyValue("sent_date", value);
                this.sentDate = value;
            }
        }
    }

}
