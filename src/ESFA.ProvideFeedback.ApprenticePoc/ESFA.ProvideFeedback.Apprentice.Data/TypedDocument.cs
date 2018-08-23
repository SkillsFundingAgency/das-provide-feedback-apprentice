﻿namespace ESFA.ProvideFeedback.Apprentice.Data
{

    using Microsoft.Azure.Documents;

    using Newtonsoft.Json;

    /// <summary>
    ///     <see cref="TypedDocument{T}" /> inherits from CosmosDB <see cref="Document" /> and provides a <see cref="Type" />
    ///     property
    ///     to assist with queries of a specific document type <see cref="T" />.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class TypedDocument<T> : Document
        where T : class
    {
        /// <summary>
        ///     Partition Key
        /// </summary>
        [JsonProperty(PropertyName = "_pk")]
        public string PartitionKey { get; set; }

        /// <summary>
        ///     Type of the class or document that is saved to CosmosDB.
        ///     Useful for querying by document of type <see cref="T" />.
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public string Type => typeof(T).Name;
    }
}