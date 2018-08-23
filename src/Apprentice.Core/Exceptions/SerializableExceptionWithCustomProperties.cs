namespace ESFA.DAS.ProvideFeedback.Apprentice.Core.Exceptions
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /// <summary>
    ///     A custom exception, with custom properties.
    ///     <remarks>Not to be used directly, please derive your exceptions from this class</remarks>
    /// </summary>
    [Serializable]
    public abstract class SerializableExceptionWithCustomProperties : Exception
    {
        private readonly string resourceName;
        private readonly IList<string> validationErrors;

        public SerializableExceptionWithCustomProperties()
        {
        }

        public SerializableExceptionWithCustomProperties(string message)
            : base(message)
        {
        }

        public SerializableExceptionWithCustomProperties(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public SerializableExceptionWithCustomProperties(string message, string resourceName, IList<string> validationErrors)
            : base(message)
        {
            this.resourceName = resourceName;
            this.validationErrors = validationErrors;
        }

        public SerializableExceptionWithCustomProperties(string message, string resourceName, IList<string> validationErrors, Exception innerException)
            : base(message, innerException)
        {
            this.resourceName = resourceName;
            this.validationErrors = validationErrors;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected SerializableExceptionWithCustomProperties(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.resourceName = info.GetString("ResourceName");
            this.validationErrors = (IList<string>)info.GetValue("ValidationErrors", typeof(IList<string>));
        }

        public string ResourceName => this.resourceName;

        public IList<string> ValidationErrors => this.validationErrors;

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue("ResourceName", this.ResourceName);

            // Note: if "List<T>" isn't serializable you may need to work out another
            //       method of adding your list, this is just for show...
            info.AddValue("ValidationErrors", this.ValidationErrors, typeof(IList<string>));

            // MUST call through to the base class to let it save its own state
            base.GetObjectData(info, context);
        }
    }
}