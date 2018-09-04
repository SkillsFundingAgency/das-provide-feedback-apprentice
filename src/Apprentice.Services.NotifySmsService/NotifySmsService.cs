namespace ESFA.DAS.ProvideFeedback.Apprentice.Services.NotifySmsService
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Interfaces;
    using ESFA.DAS.ProvideFeedback.Apprentice.Services.Helpers;
    using ESFA.DAS.ProvideFeedback.Apprentice.Services.NotifySmsService.Commands;

    using Microsoft.Extensions.Options;

    using Notify.Client;

    public class NotifySmsService : ISmsService
    {
        private readonly NotificationClient notifyClient;

        private readonly IOptions<Notify> notifyConfiguration;

        private readonly ICommandHandlerAsync<NotifySendSmsCommand> sendSmsCommandHandler;

        public NotifySmsService(
            IOptions<Notify> notifyConfiguration,
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
                .UsingClient(this.notifyClient)
                .WithSenderId(this.NotifyConfiguration.SmsSenderId)
                .SendSmsTo(destinationNumber)
                .UsingTemplate(this.NotifyConfiguration.TemplateId)
                    .AddVariable("message", messageToSend)
                    .Build()
                .WithReference(reference);

            await this.sendSmsCommandHandler.HandleAsync(command);
        }

        private NotificationClient InitializeNotifyClient()
        {
            NotificationClient client = new NotificationClient(this.NotifyConfiguration.ApiKey);
            return client;
        }
    }
}