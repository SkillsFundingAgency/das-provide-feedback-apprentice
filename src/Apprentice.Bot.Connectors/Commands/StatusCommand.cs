using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Apprentice.BotV4;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Commands
{
    public sealed class StatusCommand : AdminCommand, IBotDialogCommand
    {
        private readonly FeedbackBotAccessors _accessors;

        public StatusCommand(FeedbackBotAccessors accessors)
            : base("status")
        {
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
        }

        public override async Task<DialogTurnResult> ExecuteAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            UserInfo userInfo = await _accessors.UserProfile.GetAsync(dc.Context, () => new UserInfo(), cancellationToken);
            await dc.Context.SendActivityAsync($"{JsonConvert.SerializeObject(userInfo, Formatting.Indented )}", cancellationToken: cancellationToken);

            DialogState dialogState = await _accessors.ConversationDialogState.GetAsync(dc.Context, () => new DialogState(), cancellationToken);
            await dc.Context.SendActivityAsync($"{JsonConvert.SerializeObject(dialogState, Formatting.Indented)}", cancellationToken: cancellationToken);

            return await dc.ContinueDialogAsync(cancellationToken);
        }
    }
}