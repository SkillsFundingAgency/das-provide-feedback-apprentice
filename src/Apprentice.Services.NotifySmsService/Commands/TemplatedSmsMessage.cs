namespace ESFA.DAS.ProvideFeedback.Apprentice.Services.NotifySmsService.Commands
{
    using System.Collections.Generic;

    public class TemplatedSmsMessage
    {
        public string TemplateId { get; set; }

        private NotifySendSmsCommand parent;

        public Dictionary<string, dynamic> Variables { get; set; }

        public TemplatedSmsMessage(NotifySendSmsCommand parent)
        {
            this.parent = parent;
        }

        public TemplatedSmsMessage AddVariable(string key, dynamic value)
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