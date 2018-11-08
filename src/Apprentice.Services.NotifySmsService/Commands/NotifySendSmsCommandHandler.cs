namespace ESFA.DAS.ProvideFeedback.Apprentice.Services.NotifySmsService.Commands
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Interfaces;

    using Microsoft.Extensions.Options;

    using Notify.Client;

    public class NotifySendSmsCommandHandler : ICommandHandlerAsync<SendBasicSmsCommand>
    {
        private readonly NotificationClient client;

        private readonly Notify notifyOptions;

        public NotifySendSmsCommandHandler(
            IOptions<Notify> notifyOptions,
            NotificationClient client)
        {
            this.client = this.InitializeNotifyClient();
            this.notifyOptions = notifyOptions.Value ?? throw new ArgumentNullException(nameof(notifyOptions));

            this.TemplateId = this.notifyOptions.TemplateId;
            this.SmsSenderId = this.notifyOptions.SmsSenderId;
        }

        public string SmsSenderId { get; set; }

        public string TemplateId { get; set; }

        public SmsMessageTemplate BuildNotifyMessageTemplate(string message)
        {
            var notifyMessage = new SmsMessageTemplate(this) { TemplateId = this.TemplateId }
                .AddVariable("message", message);
            return notifyMessage;
        }

        public void Handle(SendBasicSmsCommand command)
        {
            throw new NotImplementedException();
        }

        public async Task HandleAsync(SendBasicSmsCommand command)
        {
            await this.HandleAsync(command, new CancellationToken());
        }

        public async Task HandleAsync(SendBasicSmsCommand command, CancellationToken token)
        {
            string templateId = this.TemplateId;
            string smsSenderId = this.SmsSenderId;

            var template = this.BuildNotifyMessageTemplate(command.SmsContent);
            var personalization = template.Variables;

            string mobileNumber = command.MobileNumber;
            string reference = command.Reference;

            await Task.Run(
                () => this.client.SendSms(mobileNumber, templateId, personalization, reference, smsSenderId),
                token);
        }

        public NotifySendSmsCommandHandler UsingNotifySender(string smsSenderId)
        {
            this.SmsSenderId = smsSenderId;
            return this;
        }

        public NotifySendSmsCommandHandler WithTemplateId(string templateId)
        {
            this.TemplateId = templateId;
            return this;
        }

        private NotificationClient InitializeNotifyClient()
        {
            NotificationClient client = new NotificationClient(this.notifyOptions.ApiKey);
            return client;
        }
    }
}