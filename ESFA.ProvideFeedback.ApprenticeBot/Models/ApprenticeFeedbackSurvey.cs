using System.Threading;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Prompts;
using Microsoft.Recognizers.Text;

namespace ESFA.ProvideFeedback.ApprenticeBot
{
    /// <summary>
    /// The dialogs are all configured here. Bot Framework v4.0 doesn't have a stable FormFlow implementation yet, so it needs to be inline instead of represented by IDialog implementations.
    /// TODO: revert inline dialog declaration to something similar to v3 bot framework using FormFlow
    /// TODO: modify tests to work with inline form dialogs instead of existing FormFlow based IDialog injection
    /// </summary>
    public class DefaultFeedbackSurvey : IApprenticeFeedbackSurvey
    {
        private readonly DialogSet _dialogs;

        public DefaultFeedbackSurvey()
        {
            _dialogs = new DialogSet()
                .WithRootDialog()
                .WithFeedbackQuestions()
                .WithOtherComments();

            _dialogs.Add("question", new Microsoft.Bot.Builder.Dialogs.ChoicePrompt(Culture.English) { Style = ListStyle.None });
            _dialogs.Add("freeText", new Microsoft.Bot.Builder.Dialogs.TextPrompt());
        }

        public DialogSet Current()
        {
            return _dialogs;
        }
    }

    public interface IApprenticeFeedbackSurvey
    {
        DialogSet Current();
    }
}