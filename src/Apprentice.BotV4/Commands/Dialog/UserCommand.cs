namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Commands.Dialog
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Conversation;
    using Microsoft.Bot.Builder.Dialogs;

    public abstract class UserCommand
    {
        protected UserCommand(string triggerWord)
            => this.Trigger = triggerWord ?? throw new ArgumentNullException(nameof(triggerWord));

        public string Trigger { get; }

        public abstract Task<DialogTurnResult> ExecuteAsync(DialogContext dc, CancellationToken cancellationToken);

        public bool IsTriggered(DialogContext dc, ProgressState conversationProgress)
        {
            return dc.Context.Activity.Text.ToLowerInvariant().Equals(this.Trigger);
        }
    }
}