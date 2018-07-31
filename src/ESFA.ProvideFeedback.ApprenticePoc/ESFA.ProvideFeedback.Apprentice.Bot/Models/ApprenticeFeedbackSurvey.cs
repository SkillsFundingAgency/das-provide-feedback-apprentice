using ESFA.ProvideFeedback.Apprentice.Bot.Helpers;
using ESFA.ProvideFeedback.Apprentice.Bot.Services;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Prompts;

namespace ESFA.ProvideFeedback.Apprentice.Bot.Models
{
    /// <summary>
    /// The dialogs are all configured here. Bot Framework v4.0 doesn't have a stable FormFlow implementation yet, so it needs to be inline instead of represented by IDialog implementations.
    /// TODO: revert inline dialog declaration to something similar to v3 bot framework using FormFlow
    /// TODO: modify tests to work with inline form dialogs instead of existing FormFlow based IDialog injection
    /// </summary>
    public class ApprenticeFeedbackSurvey : IApprenticeFeedbackSurvey
    {
        private readonly DialogSet _dialogs;
        private readonly IDialogFactory<DialogSet> _dialogFactory;

        public ApprenticeFeedbackSurvey(IDialogFactory<DialogSet> dialogFactory)
        {
            _dialogFactory = dialogFactory;

            _dialogs = _dialogFactory
                .BuildApprenticeFeedbackDialog()
                .WithChoicePrompt(_dialogFactory, "confirmationPrompt", ListStyle.None)
                .WithTextPrompt(_dialogFactory, "freeText")
                .WithWelcome(_dialogFactory, "start",
                    FormHelper.BuildConversationPath("daysOfTraining")
                        .WithResponse(
                            "Here’s your quarterly apprenticeship survey. You agreed to participate when you started your apprenticeship")
                        .WithResponse(
                            "It's just 3 questions and it'll really help us improve things. But if you want to opt out, please type ‘Stop’"))
                .WithBranch(_dialogFactory,
                    "daysOfTraining",
                    "Over the last 6 months, have you received at least 25 days of training?",
                    FormHelper.BuildConversationPath("trainerKnowledge")
                        .WithResponse("Thanks!"),
                    FormHelper.BuildConversationPath("trainerKnowledge")
                        .WithResponse("Okay, thanks for the feedback."))
                .WithBranch(_dialogFactory,
                    "trainerKnowledge",
                    "Is your trainer knowledgable enough to teach your course?",
                    FormHelper.BuildConversationPath("overallSatisfaction")
                        .WithResponse("Thanks"),
                    FormHelper.BuildConversationPath("overallSatisfaction")
                        .WithResponse("Okay, thanks for that."))
                .WithBranch(_dialogFactory,
                    "overallSatisfaction",
                    "Overall, are you saitisfied with your training?",
                    FormHelper.BuildConversationPath("finish")
                        .WithResponse("Great, thanks for your feedback. It’s really helpful"),
                    FormHelper.BuildConversationPath("additionalFeedback"))
                .WithFreeTextEntry(_dialogFactory,
                    "additionalFeedback",
                    "Okay, sorry to hear that. Can you tell me a bit more about it ?",
                    FormHelper.BuildConversationPath("finish")
                        .WithResponse(
                            $"Okay, thanks. I'll pass that information to our to team - it'll help us improve things."))
                .WithDynamicEnd(_dialogFactory,
                    "finish", 3,
                    FormHelper.BuildConversationEndOption()
                        .WithResponse("Keep up the good work!"),
                    FormHelper.BuildConversationEndOption()
                        .WithResponse(
                            $"If you have a problem with your apprenticeship, it’s a good idea to speak to your employer’s ‘Human Resources’ staff.")
                        .WithResponse(
                            $"If you’ve talked to them already, you might want to make a formal complaint: https://www.gov.uk/complainfurthereducationapprenticeship"));
        }

        public DialogSet Current()
        {
            return _dialogs;
        }
    }
}