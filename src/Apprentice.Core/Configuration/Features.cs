// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Azure.cs" company="Education & Skills Funding Agency">
//   Copyright © 2018 Education & Skills Funding Agency
// </copyright>
// <summary>
//   The azure config.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration
{
    /// <summary>
    /// Configuration for Azure.
    /// </summary>
    public class Features
    {
        /// <summary>
        /// Adds a delay to bot responses, and a typing indicator to fake human input
        /// </summary>
        public bool RealisticTypingDelay { get; set; }

        /// <summary>
        /// Group multiple responses into a single response back to the user
        /// </summary>
        public bool CollateResponses { get; set; }
    }
}