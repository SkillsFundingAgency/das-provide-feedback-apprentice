using Microsoft.Bot.Builder.Dialogs;

namespace ESFA.ProvideFeedback.Apprentice.Bot.Models
{
    public interface IApprenticeFeedbackSurvey
    {
        DialogSet Current();
    }
}