namespace ESFA.DAS.ProvideFeedback.Apprentice.Core.Exceptions
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /// <inheritdoc />
    [Serializable]
    public sealed class BotConnectorException : SerializableExceptionWithoutCustomProperties
    {
        public BotConnectorException()
        {
        }

        public BotConnectorException(string message)
            : base(message)
        {
        }

        public BotConnectorException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        private BotConnectorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}