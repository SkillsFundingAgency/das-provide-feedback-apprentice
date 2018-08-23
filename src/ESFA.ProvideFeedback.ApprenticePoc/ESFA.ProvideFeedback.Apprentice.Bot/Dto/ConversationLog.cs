﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConversationLog.cs" company="Education & Skills Funding Agency">
//   Copyright © 2018 Education & Skills Funding Agency
// </copyright>
// <summary>
//   The conversation log to persist.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ESFA.ProvideFeedback.Apprentice.Bot.Dto
{
    using Newtonsoft.Json;

    /// <summary>
    /// The conversation log to persist.
    /// </summary>
    internal class ConversationLog
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
        public dynamic Conversation { get; set; }

        /// <summary>
        /// Gets or sets the details of the sender.
        /// </summary>
        [JsonProperty("from")]
        public dynamic From { get; set; }

        /// <summary>
        /// Gets or sets the message sent by the user
        /// </summary>
        [JsonProperty("messageReceived")]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the recipient details
        /// </summary>
        [JsonProperty("recipient")]
        public dynamic Recipient { get; set; }

        /// <summary>
        /// Gets or sets the reply sent by the bot
        /// </summary>
        [JsonProperty("replySent")]
        public string Reply { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the exchange
        /// </summary>
        [JsonProperty("time")]
        public string Time { get; set; }
    }
}