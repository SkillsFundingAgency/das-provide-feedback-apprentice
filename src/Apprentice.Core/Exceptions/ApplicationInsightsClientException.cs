namespace ESFA.DAS.ProvideFeedback.Apprentice.Core.Exceptions
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /// <inheritdoc />
    /// <summary>
    ///     An Application Insights exception.
    /// </summary>
    [Serializable]
    public sealed class ApplicationInsightsClientException : SerializableExceptionWithCustomProperties
    {
        private readonly string instrumentationKey;

        public ApplicationInsightsClientException()
        {
        }

        public ApplicationInsightsClientException(string message)
            : base(message)
        {
        }

        public ApplicationInsightsClientException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public ApplicationInsightsClientException(string message, string instrumentationKey, string resourceName, IList<string> validationErrors)
            : base(message, resourceName, validationErrors)
        {
            this.instrumentationKey = instrumentationKey;
        }

        public ApplicationInsightsClientException(string message, string instrumentationKey, string resourceName, IList<string> validationErrors, Exception innerException)
            : base(message, resourceName, validationErrors, innerException)
        {
            this.instrumentationKey = instrumentationKey;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        private ApplicationInsightsClientException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.instrumentationKey = info.GetString("InstrumentationKey");
        }

        public string InstrumentationKey
        {
            get { return this.instrumentationKey; }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }
            info.AddValue("InstrumentationKey", this.instrumentationKey);
            base.GetObjectData(info, context);
        }
    }
}