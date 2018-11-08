namespace ESFA.DAS.ProvideFeedback.Apprentice.Services.NotifySmsService.Commands
{
    using System.Collections.Generic;

    public class SmsMessageTemplate
    {
        private readonly NotifySendSmsCommandHandler handler;

        public SmsMessageTemplate(NotifySendSmsCommandHandler handler)
        {
            this.handler = handler;
        }

        public string TemplateId { get; set; }

        public Dictionary<string, dynamic> Variables { get; set; }

        public SmsMessageTemplate AddVariable(string key, dynamic value)
        {
            this.Variables.Add(key, value);
            return this;
        }

        public NotifySendSmsCommandHandler Build()
        {
            return this.handler;
        }
    }
}