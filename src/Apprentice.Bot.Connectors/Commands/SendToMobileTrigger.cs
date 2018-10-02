namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Middleware;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration;

    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Extensions.Options;

    using Newtonsoft.Json;

    public sealed class SendToMobileTrigger : AdminCommand, IBotDialogCommand
    {
        private readonly ISmsQueueProvider queue;

        private readonly Notify notifyConfig;

        public SendToMobileTrigger(ISmsQueueProvider queue, IOptions<Notify> notifyConfig)
            : base("^invite (44)(7)\\d{9}$")
        {
            this.queue = queue;
            this.notifyConfig = notifyConfig.Value;
        }

        public override async Task ExecuteAsync(DialogContext dc)
        {
            string message = dc.Context.Activity.Text.ToLowerInvariant();

            var strings = message.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            var mobileNumber = strings[1];

            var trigger = new SmsConversationTrigger()
                              {
                                  Id = Guid.NewGuid().ToString(),
                                  SourceNumber = mobileNumber,
                                  DestinationNumber = null,
                                  Message = "start afb-v5",
                                  TimeStamp = DateTime.UtcNow
                              };

            var payload = new KeyValuePair<string, SmsConversationTrigger>("bot-manual-trigger", trigger);

            await this.queue.SendAsync(JsonConvert.SerializeObject(payload), this.notifyConfig.IncomingMessageQueueName);
            await dc.Context.SendActivity($"OK. Sending survey to {mobileNumber}");
        }

        public override bool IsTriggered(DialogContext dc)
        {
            var utterance = dc.Context.Activity.Text.ToLowerInvariant();
            return Regex.IsMatch(utterance, this.Trigger);
        }
    }
}