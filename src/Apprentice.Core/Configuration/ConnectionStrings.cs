// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConnectionStrings.cs" company="Education & Skills Funding Agency">
//   Copyright © 2018 Education & Skills Funding Agency
// </copyright>
// <summary>
//   Settings class for connection strings.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration
{
    /// <summary>
    /// Settings class for connection strings.
    /// </summary>
    public class ConnectionStrings
    {
        public string StorageAccount { get; set; }

        public string ServiceBus { get; set; }
    }
}