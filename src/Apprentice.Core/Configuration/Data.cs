// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Data.cs" company="Education & Skills Funding Agency">
//   Copyright © 2018 Education & Skills Funding Agency
// </copyright>
// <summary>
//   The configuration class for data access.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration
{
    /// <summary>
    /// The configuration class for data access.
    /// </summary>
    public class Data
    {
        /// <summary>
        /// Gets or sets the name of the database to be used.
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// Gets or sets the name of the feedback table.
        /// </summary>
        public string FeedbackTable { get; set; }

        /// <summary>
        /// Gets or sets the name of the conversation log table.
        /// </summary>
        public string ConversationLogTable { get; set; }

        /// <summary>
        /// Gets or sets the partition key for the Conversation Log
        /// </summary>
        public string ConversationLogPartitionKey { get; set; }

        /// <summary>
        /// Gets or sets the name of session state table.
        /// </summary>
        public string SessionStateTable { get; set; }
    }
}