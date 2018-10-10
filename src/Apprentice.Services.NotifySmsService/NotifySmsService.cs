namespace ESFA.DAS.ProvideFeedback.Apprentice.Services.NotifySmsService
{
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Interfaces;
    using ESFA.DAS.ProvideFeedback.Apprentice.Services.Helpers;
    using ESFA.DAS.ProvideFeedback.Apprentice.Services.NotifySmsService.Commands;

    using Microsoft.Extensions.Options;

    using Notify.Client;

    using NotifyConfig = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Notify;

    public class NotifySmsService : ISmsService
    {
        private readonly NotificationClient notifyClient;

        private readonly IOptions<NotifyConfig> notifyConfiguration;

        private readonly ICommandHandlerAsync<NotifySendSmsCommand> sendSmsCommandHandler;

        public NotifySmsService(
            IOptions<NotifyConfig> notifyConfiguration,
            ICommandHandlerAsync<NotifySendSmsCommand> sendSmsCommandHandler)
        {
            this.notifyConfiguration = notifyConfiguration;
            this.sendSmsCommandHandler = sendSmsCommandHandler;
            this.notifyClient = this.InitializeNotifyClient();
        }

        public Notify NotifyConfiguration => this.notifyConfiguration.Value;

        public void SendSms(string destinationNumber, string messageToSend, string reference = null)
        {
            AsyncHelper.RunSync(() => this.SendSmsAsync(destinationNumber, messageToSend));
        }

        public async Task SendSmsAsync(string destinationNumber, string messageToSend, string reference = null)
        {
            NotifySendSmsCommand command = new NotifySendSmsCommand()
                .SendSmsTo(destinationNumber)
                .UsingNotifySender(this.NotifyConfiguration.SmsSenderId)
                .UsingNotifyTemplate(this.NotifyConfiguration.TemplateId)
                    .AddVariable("message", messageToSend)
                    .Build()
                .WithReference(reference)
                .UsingNotifyClient(this.notifyClient);

            await this.sendSmsCommandHandler.HandleAsync(command);
        }

        private NotificationClient InitializeNotifyClient()
        {
            NotificationClient client = new NotificationClient(this.NotifyConfiguration.ApiKey);
            return client;
        }
    }
}