namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Commands.Dialog
{
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Commands.Dialog;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration;

    using Microsoft.Bot.Builder.Dialogs;

    public sealed class DisplayVersionCommand : AdminCommand, IBotDialogCommand
    {
        public DisplayVersionCommand(Bot botConfiguration)
            : base("bot--version", botConfiguration)
        {
        }

        public override async Task<DialogTurnResult> ExecuteAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            var versionNumber = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            await dc.Context.SendActivityAsync($"Bertie v{versionNumber}", cancellationToken: cancellationToken);
            return new DialogTurnResult(DialogTurnStatus.Waiting);
        }
    }
}