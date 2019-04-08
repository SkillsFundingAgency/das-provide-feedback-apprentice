using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs
{
    public interface IFeedbackBotStateRepository
    {
        IStatePropertyAccessor<DialogState> ConversationDialogState { get; set; }

        IStatePropertyAccessor<UserProfile> UserProfile { get; set; }

        ConversationState ConversationState { get; }

        UserState UserState { get; }
    }
}
