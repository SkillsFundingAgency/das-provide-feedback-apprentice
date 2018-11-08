namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs
{
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Interfaces;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Models;

    using Microsoft.Bot.Builder.Dialogs;

    public interface IDialogFactory
    {
        /// <summary>
        /// Create a survey step DialogContainer
        /// </summary>
        /// <typeparam name="T">The type of step to create</typeparam>
        /// <param name="stepDefinition">The step DTO</param>
        /// <returns>a new <see cref="ComponentDialog"/></returns>
        T Create<T>(ISurveyStepDefinition stepDefinition)
            where T : ComponentDialog;

        /// <summary>
        /// Create a survey DialogContainer
        /// </summary>
        /// <typeparam name="T">The type of survey to create</typeparam>
        /// <param name="surveyDefinition">The survey DTO</param>
        /// <returns>a new <see cref="ComponentDialog"/></returns>
        T Create<T>(ISurveyDefinition surveyDefinition)
            where T : ComponentDialog;
    }
}