namespace ESFA.DAS.ProvideFeedback.Apprentice.Core.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     A custom exception, without custom properties.
    ///     <remarks>Not to be used directly, please derive your exceptions from this class</remarks>
    /// </summary>
    [Serializable]
    public abstract class SerializableExceptionWithoutCustomProperties : Exception
    {
        public SerializableExceptionWithoutCustomProperties()
        {
        }

        public SerializableExceptionWithoutCustomProperties(string message)
            : base(message)
        {
        }

        public SerializableExceptionWithoutCustomProperties(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected SerializableExceptionWithoutCustomProperties(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}