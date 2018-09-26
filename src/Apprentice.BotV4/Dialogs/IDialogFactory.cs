namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Dialogs
{
    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models;

    using Microsoft.Bot.Builder.Dialogs;

    public interface IDialogFactory
    {
        /// <summary>
        /// Create a survey step DialogContainer
        /// </summary>
        /// <typeparam name="T">The type of step to create</typeparam>
        /// <param name="step">The step DTO</param>
        /// <returns>a new <see cref="ComponentDialog"/></returns>
        T Create<T>(ISurveyStep step)
            where T : ComponentDialog;

        /// <summary>
        /// Create a survey DialogContainer
        /// </summary>
        /// <typeparam name="T">The type of survey to create</typeparam>
        /// <param name="survey">The survey DTO</param>
        /// <returns>a new <see cref="ComponentDialog"/></returns>
        T Create<T>(ISurvey survey)
            where T : ComponentDialog;
    }
}