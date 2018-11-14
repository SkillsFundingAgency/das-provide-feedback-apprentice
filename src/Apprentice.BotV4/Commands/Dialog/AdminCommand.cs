namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Commands.Dialog
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Conversation;
    using Microsoft.Bot.Builder.Dialogs;

    using BotConfiguration = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Bot;

    public abstract class AdminCommand
    {
        protected readonly BotConfiguration botConfiguration;

        protected AdminCommand(string triggerWord, BotConfiguration botConfiguration)
        {
            this.Trigger = triggerWord ?? throw new ArgumentNullException(nameof(triggerWord));
            this.botConfiguration = botConfiguration;
        }

        public string Trigger { get; }

        public abstract Task<DialogTurnResult> ExecuteAsync(DialogContext dc, CancellationToken cancellationToken);

        public virtual bool IsTriggered(DialogContext dc, ProgressState conversationProgress)
        {
            // TODO: check auth
            return this.botConfiguration.AdminCommandsSplit.Contains(this.Trigger)
                && dc.Context.Activity.Text.ToLowerInvariant().StartsWith(this.Trigger, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}