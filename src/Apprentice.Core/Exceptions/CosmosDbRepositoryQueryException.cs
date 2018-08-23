namespace ESFA.DAS.ProvideFeedback.Apprentice.Core.Exceptions
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /// <inheritdoc />
    /// <summary>
    ///     An CosmosDbRepositoryQuery exception.
    /// </summary>
    [Serializable]
    public sealed class CosmosDbRepositoryQueryException : SerializableExceptionWithoutCustomProperties
    {
        public CosmosDbRepositoryQueryException()
        {
        }

        public CosmosDbRepositoryQueryException(string message)
            : base(message)
        {
        }

        public CosmosDbRepositoryQueryException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        private CosmosDbRepositoryQueryException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}