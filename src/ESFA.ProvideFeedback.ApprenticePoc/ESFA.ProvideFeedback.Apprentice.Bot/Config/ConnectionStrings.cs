// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConnectionStrings.cs" company="Education & Skills Funding Agency">
//   Copyright © 2018 Education & Skills Funding Agency
// </copyright>
// <summary>
//   Settings class for connection strings.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ESFA.ProvideFeedback.Apprentice.Bot.Config
{
    /// <summary>
    /// Settings class for connection strings.
    /// </summary>
    public class ConnectionStrings
    {
        /// <summary>
        /// Gets or sets the connection string for the azure storage account.
        /// </summary>
        public string StorageAccount { get; set; }
    }
}