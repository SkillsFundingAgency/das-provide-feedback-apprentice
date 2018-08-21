// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApprenticeFeedbackV3.cs" company="Education & Skills Funding Agency">
//   Copyright © 2018 Education & Skills Funding Agency
// </copyright>
// <summary>
//   The dialogs are all configured here. Bot Framework v4.0 doesn't have a stable FormFlow implementation yet, so it needs to be inline instead of represented by IDialog implementations.
//   TODO: revert inline dialog declaration to something similar to v3 bot framework using FormFlow
//   TODO: modify tests to work with inline form dialogs instead of existing FormFlow based IDialog injection
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ESFA.ProvideFeedback.Apprentice.Bot.Models
{
    using ESFA.ProvideFeedback.Apprentice.Bot.Helpers;
    using ESFA.ProvideFeedback.Apprentice.Bot.Models;
    using ESFA.ProvideFeedback.Apprentice.Bot.Services;

    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Prompts;

    /// <inheritdoc />
    /// <summary>
    ///     The dialogs are all configured here. Bot Framework v4.0 doesn't have a stable FormFlow implementation yet, so it needs to be inline instead of represented by IDialog implementations.
    ///     TODO: revert inline dialog declaration to something similar to v3 bot framework using FormFlow
    ///     TODO: modify tests to work with inline form dialogs instead of existing FormFlow based IDialog injection
    /// </summary>
    public class ApprenticeFeedbackV3 : IApprenticeFeedbackSurvey
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ApprenticeFeedbackV3"/> class. 
        ///     Creates a simple Apprentice Feedback survey conversation
        /// </summary>
        /// <param name="dialogFactory"> the factory to be used for creating dialogs </param>
        /// <param name="resources">
        ///     The object to be used for string localization.
        ///     Must match the class name of the <see cref="IApprenticeFeedbackSurvey"/> implementation, and have a corresponding Resources (*.resx) file in the Resources folder.
        /// </param>
        public ApprenticeFeedbackV3(IDialogFactory<DialogSet> dialogFactory, IApprenticeFeedbackResources resources)
        {
            // Create the steps
            IDialogStep step1 = FormHelper.BuildConversationPath("daysOfTraining").WithResponse(resources.IntroWelcome)
                .WithResponse(resources.IntroOptOut);

            IDialogStep step2Positive = FormHelper.BuildConversationPath("trainerKnowledge")
                .WithResponse(resources.ResponsesPositive01);

            IDialogStep step2Negative = FormHelper.BuildConversationPath("trainerKnowledge")
                .WithResponse(resources.ResponsesNegative01);

            IDialogStep step3Positive = FormHelper.BuildConversationPath("overallSatisfaction")
                .WithResponse(resources.ResponsesPositive02);

            IDialogStep step3Negative = FormHelper.BuildConversationPath("overallSatisfaction")
                .WithResponse(resources.ResponsesNegative02);

            IDialogStep step4Positive = FormHelper.BuildConversationPath("finish")
                .WithResponse(resources.ResponsesItsReallyHelpful);

            IDialogStep step4Negative = FormHelper.BuildConversationPath("finish")
                .WithResponse(resources.ResponsesSorryToHearThat);

            IDialogStep positiveEnd =
                FormHelper.BuildConversationEndOption().WithResponse(resources.FinishKeepUpTheGoodWork);

            IDialogStep negativeEnd = FormHelper.BuildConversationEndOption()
                .WithResponse(resources.FinishSpeakToYourEmployer).WithResponse(resources.FinishFormalComplaint);

            // Build the conversation, tying the steps together
            this.Dialogs = dialogFactory.Conversation()
                .WithChoicePrompt(dialogFactory, "confirmationPrompt", ListStyle.None)
                .WithWelcome(dialogFactory, "start", step1)
                .WithBranch(
                    dialogFactory,
                    "daysOfTraining",
                    resources.QuestionsDaysOfTraining,
                    step2Positive,
                    step2Negative)
                .WithBranch(
                    dialogFactory,
                    "trainerKnowledge",
                    resources.QuestionsTrainerKnowledge,
                    step3Positive,
                    step3Negative)
                .WithBranch(
                    dialogFactory,
                    "overallSatisfaction",
                    resources.QuestionsOverallSatisfaction,
                    step4Positive,
                    step4Negative).WithDynamicEnd(dialogFactory, "finish", 3, positiveEnd, negativeEnd);
        }

        /// <inheritdoc />
        public DialogSet Dialogs { get; }
    }
}