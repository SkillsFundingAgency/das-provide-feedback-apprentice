namespace ESFA.DAS.ProvideFeedback.Apprentice.Core.Exceptions
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /// <inheritdoc />
    [Serializable]
    public sealed class ConversationLockedException : SerializableExceptionWithoutCustomProperties
    {
        public ConversationLockedException()
        {
        }

        public ConversationLockedException(string message)
            : base(message)
        {
        }

        public ConversationLockedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        private ConversationLockedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}