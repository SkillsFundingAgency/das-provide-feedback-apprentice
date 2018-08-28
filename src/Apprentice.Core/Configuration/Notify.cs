// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Notify.cs" company="Education & Skills Funding Agency">
//   Copyright © 2018 Education & Skills Funding Agency
// </copyright>
// <summary>
//   The configuration settings for the Notify service
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ESFA.DAS.ProvideFeedback.Apprentice.Infrastructure.Configuration
{
    /// <summary>
    /// The configuration settings for the Notify service
    /// </summary>
    public class Notify
    {
        public string ApiKey { get; set; }

        public string TemplateId { get; set; }

        public string SmsSenderId { get; set; }

        /// <summary>
        /// Gets or sets the name of the outgoing SMS queue.
        /// </summary>
        public string OutgoingMessageQueueName { get; set; }

        /// <summary>
        /// Gets or sets the name of the incoming SMS queue.
        /// </summary>
        public string IncomingMessageQueueName { get; set; }
    }
}