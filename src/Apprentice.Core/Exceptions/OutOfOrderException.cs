namespace ESFA.DAS.ProvideFeedback.Apprentice.Core.Exceptions
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /// <inheritdoc />
    [Serializable]
    public sealed class OutOfOrderException : SerializableExceptionWithoutCustomProperties
    {
        public OutOfOrderException()
        {
        }

        public OutOfOrderException(string message)
            : base(message)
        {
        }

        public OutOfOrderException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        private OutOfOrderException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}