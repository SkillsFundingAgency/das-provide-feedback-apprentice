namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Commands.Dialog
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Commands.Dialog;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Conversation;
    using ESFA.DAS.ProvideFeedback.Apprentice.Services;

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

        /// <inheritdoc />
        public override async Task<DialogTurnResult> ExecuteAsync(DialogContext dc, CancellationToken cancellationToken)
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
            await dc.Context.SendActivityAsync($"OK. Sending survey to {mobileNumber}", cancellationToken: cancellationToken);

            return await dc.ContinueDialogAsync(cancellationToken);
        }

        /// <inheritdoc />
        public override bool IsTriggered(DialogContext dc)
        {
            var utterance = dc.Context.Activity.Text.ToLowerInvariant();
            return Regex.IsMatch(utterance, this.Trigger);
        }
    }
}