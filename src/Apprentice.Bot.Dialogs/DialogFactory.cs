namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Feedback.Survey;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Interfaces;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Exceptions;

    using Microsoft.Bot.Builder.Dialogs;

    public class DialogFactory : IDialogFactory
    {
        private readonly IEnumerable<IComponentBuilder<ComponentDialog>> stepBuilders;

        /// <inheritdoc />
        public DialogFactory(
            IEnumerable<IComponentBuilder<ComponentDialog>> stepBuilders)
        {
            this.stepBuilders = stepBuilders ?? throw new ArgumentNullException(nameof(stepBuilders));
        }

        /// <inheritdoc />
        public T Create<T>(ISurveyStepDefinition stepDefinition)
            where T : ComponentDialog => (T)this.CreateDialogStep(stepDefinition);

        /// <inheritdoc />
        public T Create<T>(ISurveyDefinition surveyDefinition)
            where T : ComponentDialog => (T)this.CreateSurvey(surveyDefinition);

        private ComponentDialog CreateDialogStep(ISurveyStepDefinition stepDefinition)
        {
            try
            {
                var stepBuilder = this.stepBuilders.First(s => s.Matches(stepDefinition));

                ComponentDialog step = stepBuilder.Create(stepDefinition);

                return step;
            }
            catch (Exception ex)
            {
                throw new DialogFactoryException($"Could not create ComponentDialog : Unsupported type [{stepDefinition.GetType().FullName}]");
            }
        }

        private ComponentDialog CreateSurvey(ISurveyDefinition surveyDefinition)
        {
            var dialogs = new List<Dialog>();
            foreach (ISurveyStepDefinition stepDefinition in surveyDefinition.StepDefinitions)
            {
                // register each step with the dialogs stack
                dialogs.Add(this.CreateDialogStep(stepDefinition));
            }

            return new SurveyDialog(surveyDefinition.Id)
                .WithDialogSteps(dialogs)
                .Build();
        }
    }
}