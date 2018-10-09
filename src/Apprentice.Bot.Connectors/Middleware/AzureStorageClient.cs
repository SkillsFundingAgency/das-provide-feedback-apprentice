namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Middleware
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Options;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Queue;

    using ConnectionStrings = Core.Configuration.ConnectionStrings;
    using NotifyConfiguration = Core.Configuration.Notify;

    public class AzureStorageClient : ISmsQueueProvider
    {
        private readonly CloudQueueClient queueClient;

        private readonly NotifyConfiguration notifyConfig;

        private readonly ConnectionStrings connectionStrings;

        public AzureStorageClient(IOptions<NotifyConfiguration> notifyConfigOptions, IOptions<ConnectionStrings> connectionStringsOptions)
        {
            this.notifyConfig = notifyConfigOptions.Value;
            this.connectionStrings = connectionStringsOptions.Value;

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(this.connectionStrings.StorageAccount);
            this.queueClient = storageAccount.CreateCloudQueueClient();
        }

        ~AzureStorageClient()
        {
            // dispose here
        }


        public void Send(string message, string queueName)
        {
            throw new NotImplementedException();
        }

        public async Task SendAsync(string message, string queueName)
        {
            CloudQueue messageQueue = this.queueClient.GetQueueReference(queueName);
            await messageQueue.CreateIfNotExistsAsync();

            CloudQueueMessage queueMessage = new CloudQueueMessage(message);
            await messageQueue.AddMessageAsync(queueMessage);
        }
    }
}