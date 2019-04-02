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

    using BotConfiguration = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Bot;

    public sealed class SendToMobileTrigger : AdminCommand, IBotDialogCommand
    {
        private readonly ISmsQueueProvider queue;

        private readonly Notify notifyConfig;

        public SendToMobileTrigger(ISmsQueueProvider queue, IOptions<Notify> notifyConfig, BotConfiguration botConfiguration)
            : base("^bot_invite_mobile (44)(7)\\d{9}$", botConfiguration)
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
                Message = "bot_dialog_start afb-v5",
                DateReceived = DateTime.UtcNow,
                UniqueLearnerNumber = "uln_here",
                ApprenticeshipStartDate = DateTime.Now.AddYears(-1),
                StandardCode = 23,
            };

            await this.queue.SendAsync(dc.Context.Activity.Conversation.Id, JsonConvert.SerializeObject(trigger), this.notifyConfig.IncomingMessageQueueName);
            await dc.Context.SendActivityAsync($"OK. Sending survey to {mobileNumber}", cancellationToken: cancellationToken);

            return await dc.ContinueDialogAsync(cancellationToken);
        }

        /// <inheritdoc />
        public override bool IsTriggered(DialogContext dc, ProgressState conversationProgress)
        {
            var utterance = dc.Context.Activity.Text.ToLowerInvariant();
            return Regex.IsMatch(utterance, this.Trigger);
        }
    }
}