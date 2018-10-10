namespace ESFA.DAS.ProvideFeedback.Apprentice.Services.NotifySmsService.Commands
{
    using System.Collections.Generic;

    public class SmsMessageTemplate
    {
        private readonly NotifySendSmsCommand parent;

        public SmsMessageTemplate(NotifySendSmsCommand parent)
        {
            this.parent = parent;
        }

        public string TemplateId { get; set; }

        public Dictionary<string, dynamic> Variables { get; set; }

        public SmsMessageTemplate AddVariable(string key, dynamic value)
        {
            this.Variables.Add(key, value);
            return this;
        }

        public NotifySendSmsCommand Build()
        {
            return this.parent;
        }
    }
}