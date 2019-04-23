using System;
using ESFA.DAS.ProvideFeedback.Apprentice.Domain.Messages;
using Microsoft.Azure.ServiceBus;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Services
{
    public class QueueClientFactory : IQueueClientFactory
    {
        private string _incomingMessagesQueueName = "sms-incoming-messages";
        private string _outgoingMessagesQueueName = "sms-outgoing-messages";
        private readonly string _servicebusConnection;

        public QueueClientFactory(ISettingService settingService)
        {
            _servicebusConnection = settingService.Get("ServiceBusConnection");
            _incomingMessagesQueueName = settingService.Get("IncomingMessageQueueName");
            _outgoingMessagesQueueName = settingService.Get("OutgoingMessageQueueName");
        }

        public IQueueClient Create<T>() where T : Message
        {
            var type = typeof(T);

            if (type.IsAssignableFrom(typeof(SmsIncomingMessage)))
            {
                return CreateQueueClient(_incomingMessagesQueueName);
            }

            if (type.IsAssignableFrom(typeof(SmsOutgoingMessage)))
            {
                return CreateQueueClient(_outgoingMessagesQueueName);
            }
            else
            {
                throw new NotImplementedException($"Creating a queue client for type {nameof(type)} is not implemented");
            }
        }

        private IQueueClient CreateQueueClient(string queueName)
        {
            return new QueueClient(_servicebusConnection, queueName);
        }
    }
}
