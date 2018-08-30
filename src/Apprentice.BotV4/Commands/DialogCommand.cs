namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4
{
    using System;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Commands;

    using Microsoft.Bot.Builder.Core.Extensions;
    using Microsoft.Bot.Builder.Dialogs;

    public abstract class UserCommand : IBotDialogCommand
    {
        protected UserCommand(string triggerWord)
            => this.Trigger = triggerWord ?? throw new ArgumentNullException(nameof(triggerWord));

        public string Trigger { get; }

        public abstract Task ExecuteAsync(DialogContext dc);

        public bool IsTriggered(DialogContext dc)
        {
            return dc.Context.Activity.Text.ToLowerInvariant().StartsWith(this.Trigger);
        }
    }

    public abstract class AdminCommand : IBotDialogCommand
    {
        protected AdminCommand(string triggerWord) 
            => this.Trigger = triggerWord ?? throw new ArgumentNullException(nameof(triggerWord));

        public string Trigger { get; }

        public abstract Task ExecuteAsync(DialogContext dc);

        public bool IsTriggered(DialogContext dc)
        {
            // TODO: check auth
            return dc.Context.Activity.Text.ToLowerInvariant().StartsWith(this.Trigger);
        }
    }
}