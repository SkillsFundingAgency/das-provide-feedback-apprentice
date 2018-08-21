using Microsoft.Bot.Builder.Dialogs;

namespace ESFA.ProvideFeedback.Apprentice.Bot.Models
{
    /// <summary>
    /// Represents an apprentice feedback survey dialog set.
    /// </summary>
    public interface IApprenticeFeedbackSurvey
    {
        /// <summary>
        /// Gets the dialog collection.
        /// </summary>
        DialogSet Dialogs { get; }
    }
}