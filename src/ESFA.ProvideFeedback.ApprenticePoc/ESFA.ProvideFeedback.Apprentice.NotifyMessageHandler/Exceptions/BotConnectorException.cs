namespace ESFA.ProvideFeedback.Apprentice.NotifyMessageHandler.Exceptions
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class BotConnectorException : Exception
    {
        public BotConnectorException()
        {
        }

        public BotConnectorException(string message)
            : base(message)
        {
        }

        public BotConnectorException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected BotConnectorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.ResourceReferenceProperty = info.GetString("ResourceReferenceProperty");
        }

        public string ResourceReferenceProperty { get; set; }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue("ResourceReferenceProperty", this.ResourceReferenceProperty);
            base.GetObjectData(info, context);
        }
    }
}