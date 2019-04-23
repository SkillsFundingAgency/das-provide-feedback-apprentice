namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Middleware
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration;
    using ESFA.DAS.ProvideFeedback.Apprentice.Services;

    using Microsoft.Azure.ServiceBus;
    using Microsoft.Extensions.Options;

    public class AzureServiceBusClient : ISmsQueueProvider
    {
        private readonly List<IQueueClient> queueClients = new List<IQueueClient>();

        private readonly Notify notifyConfig;

        private readonly ConnectionStrings connectionStrings;

        public AzureServiceBusClient(IOptions<Notify> notifyConfigOptions, IOptions<ConnectionStrings> connectionStrings)
        {
            this.notifyConfig = notifyConfigOptions.Value;
            this.connectionStrings = connectionStrings.Value;

            this.queueClients.Add(new QueueClient(this.connectionStrings.ServiceBus, this.notifyConfig.OutgoingMessageQueueName));
            this.queueClients.Add(new QueueClient(this.connectionStrings.ServiceBus, this.notifyConfig.IncomingMessageQueueName));
        }

        ~AzureServiceBusClient()
        {
            // dispose here
        }

        public IEnumerable<string> QueueNames
        {
            get
            {
                return this.queueClients.Select(q => q.QueueName);
            }
        }

        public void Send(string message, string queueName)
        {
            throw new NotImplementedException();
        }

        public async Task SendAsync(string conversationId, Message message, string queueName)
        {
            try
            {
                IQueueClient client = this.queueClients.First(q => q.QueueName == queueName);

                await client.SendAsync(message);
            }
            catch (InvalidOperationException e)
            {
                throw new Exception($"Could not find a Service Bus queue with the name {queueName}", e);
            }
        }
    }
}