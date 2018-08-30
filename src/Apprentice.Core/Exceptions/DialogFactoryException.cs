namespace ESFA.DAS.ProvideFeedback.Apprentice.Core.Exceptions
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public sealed class DialogFactoryException : SerializableExceptionWithoutCustomProperties
    {
        public DialogFactoryException()
        {
        }

        public DialogFactoryException(string message)
            : base(message)
        {
        }

        public DialogFactoryException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        private DialogFactoryException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}