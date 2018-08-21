// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Azure.cs" company="Education & Skills Funding Agency">
//   Copyright © 2018 Education & Skills Funding Agency
// </copyright>
// <summary>
//   The azure config.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace ESFA.ProvideFeedback.Apprentice.Bot.Config
{
    using System.Collections.Generic;

    using Microsoft.Extensions.Options;

    /// <summary>
    /// Configuration for Azure.
    /// </summary>
    public class Azure
    {
        /// <summary>
        /// Gets or sets the endpoint for Cosmos Db..
        /// </summary>
        public string CosmosEndpoint { get; set; }

        /// <summary>
        /// Gets or sets access key for Cosmos Db..
        /// </summary>
        public string CosmosKey { get; set; }

        /// <summary>
        /// Gets or sets the default RU's for Cosmos collections
        /// </summary>
        public int CosmosDefaultThroughput { get; set; } = 1000;
    }
}