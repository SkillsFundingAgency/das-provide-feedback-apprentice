// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OutgoingSms.cs" company="Education & Skills Funding Agency">
//   Copyright © 2018 Education & Skills Funding Agency
// </copyright>
// <summary>
//   Represents an SMS message to be sent using the Notify service.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ESFA.DAS.ProvideFeedback.Apprentice.Domain.Dto
{
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Conversation;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents an SMS message to be sent using the Notify service.
    /// </summary>
    public class OutgoingSms
    {
        /// <summary>
        /// Gets or sets the channel specific data. In the case of Slack, this would include chat name, account name, etc.
        /// </summary>
        [JsonProperty("channelData")]
        public dynamic ChannelData { get; set; }

        /// <summary>
        /// Gets or sets the channel id.
        /// </summary>
        [JsonProperty("channelId")]
        public string ChannelId { get; set; }

        /// <summary>
        /// Gets or sets the conversation specific data. Includes items like the conversation Id.
        /// </summary>
        [JsonProperty("conversation")]
        public BotConversation Conversation { get; set; }

        /// <summary>
        /// Gets or sets the details of the sender.
        /// </summary>
        [JsonProperty("from")]
        public Participant From { get; set; }

        /// <summary>
        /// Gets or sets the message sent by the user
        /// </summary>
        [JsonProperty("messageReceived")]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the recipient details
        /// </summary>
        [JsonProperty("recipient")]
        public Participant Recipient { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the exchange
        /// </summary>
        [JsonProperty("time")]
        public string Time { get; set; }
    }
}