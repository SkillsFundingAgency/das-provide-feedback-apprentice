namespace ESFA.DAS.ProvideFeedback.Apprentice.Core.Exceptions
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /// <inheritdoc />
    [Serializable]
    public sealed class PreviousMessageNotSentException : SerializableExceptionWithoutCustomProperties
    {
        public PreviousMessageNotSentException()
        {
        }

        public PreviousMessageNotSentException(string message)
            : base(message)
        {
        }

        public PreviousMessageNotSentException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        private PreviousMessageNotSentException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}