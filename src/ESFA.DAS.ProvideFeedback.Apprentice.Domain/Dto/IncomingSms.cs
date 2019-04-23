namespace ESFA.DAS.ProvideFeedback.Apprentice.Domain.Dto
{
    using System;
    using Newtonsoft.Json;

    public enum SmsType
    {
        NotifySms,
        SurveyInvitation
    }

    [Serializable]
    public class IncomingSms
    {
        [JsonProperty("date_received")]
        public DateTime DateReceived { get; set; }

        [JsonProperty("destination_number")]
        public string DestinationNumber { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("source_number")]
        public string SourceNumber { get; set; }

        [JsonProperty("type")]
        public SmsType Type { get; set; }

        [JsonProperty("unique_learner_number")]
        public string UniqueLearnerNumber { get; set; }

        [JsonProperty("standard_code")]
        public int? StandardCode { get; set; }

        [JsonProperty("apprenticeship_start_date")]
        public DateTime? ApprenticeshipStartDate { get; set; }
    }
}