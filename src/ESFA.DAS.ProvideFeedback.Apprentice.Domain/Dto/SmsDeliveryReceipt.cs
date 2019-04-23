namespace ESFA.DAS.ProvideFeedback.Apprentice.Domain.Dto
{
    using System;

    using Newtonsoft.Json;

    [Serializable]
    public class SmsDeliveryReceipt
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("reference")]
        public string Reference { get; set; }

        [JsonProperty("to")]
        public string To { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedDate { get; set; }

        [JsonProperty("completed_at")]
        public DateTime CompletedDate { get; set; }

        [JsonProperty("sent_at")]
        public DateTime SentDate { get; set; }

        [JsonProperty("notification_type")]
        public string NotificationType { get; set; }
    }
}