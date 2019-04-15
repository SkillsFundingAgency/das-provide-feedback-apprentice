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

        public IQueueClient CreateIncomingSmsQueueClient()
        {
            return new QueueClient(_servicebusConnection, _incomingMessagesQueueName);
        }

        public IQueueClient CreateOutgoingSmsQueueClient()
        {
            return new QueueClient(_servicebusConnection, _outgoingMessagesQueueName);
        }
    }
}
