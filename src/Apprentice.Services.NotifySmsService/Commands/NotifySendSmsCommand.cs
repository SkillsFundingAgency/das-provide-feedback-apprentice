namespace ESFA.DAS.ProvideFeedback.Apprentice.Services.NotifySmsService.Commands
{
    using System.Threading;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Interfaces;

    using Notify.Client;

    public class NotifySendSmsCommand : ICommandAsync
    {
        public NotificationClient Client { get; set; }

        public string MobileNumber { get; set; }

        public TemplatedSmsMessage Template { get; set; }

        public string ReferenceId { get; set; }

        public string SmsSenderId { get; set; }

        public void Execute()
        {
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            string mobileNumber = this.MobileNumber;
            string templateId = this.Template.TemplateId;
            var personalization = this.Template.Variables;
            string reference = this.ReferenceId;
            string smsSenderId = this.SmsSenderId;

            await Task.Run(
                () => this.Client.SendSms(mobileNumber, templateId, personalization, reference, smsSenderId),
                cancellationToken);
        }

        public NotifySendSmsCommand UsingClient(NotificationClient notifyClient)
        {
            this.Client = notifyClient;
            return this;
        }

        public NotifySendSmsCommand WithSenderId(string smsSenderId)
        {
            this.SmsSenderId = smsSenderId;
            return this;
        }

        public NotifySendSmsCommand WithReference(string referenceId)
        {
            this.ReferenceId = referenceId;
            return this;
        }

        public TemplatedSmsMessage UsingTemplate(string templateId)
        {
            this.Template = new TemplatedSmsMessage(this) { TemplateId = templateId };
            return this.Template;
        }

        public NotifySendSmsCommand SendSmsTo(string mobileNumber)
        {
            this.MobileNumber = mobileNumber;
            return this;
        }
    }
}