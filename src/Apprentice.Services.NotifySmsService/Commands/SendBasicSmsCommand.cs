namespace ESFA.DAS.ProvideFeedback.Apprentice.Services.NotifySmsService.Commands
{
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Interfaces;

    public class SendBasicSmsCommand : ICommandAsync
    {
        public string MobileNumber { get; set; }

        public string SmsContent { get; set; }

        public string Reference { get; set; }

        public SendBasicSmsCommand To(string mobileNumber)
        {
            this.MobileNumber = mobileNumber;
            return this;
        }

        public SendBasicSmsCommand Message(string message)
        {
            this.SmsContent = message;
            return this;
        }


        public SendBasicSmsCommand WithReference(string referenceId)
        {
            this.Reference = referenceId;
            return this;
        }
    }
}