namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Feedback.Components;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Feedback.Survey;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Exceptions;
    using ESFA.DAS.ProvideFeedback.Apprentice.Data.Repositories;

    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Extensions.Options;

    using BotSettings = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Bot;
    using FeatureToggles = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Features;

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
            where T : ComponentDialog => (T)this.CreateStep(stepDefinition);

        /// <inheritdoc />
        public T Create<T>(ISurvey survey)
            where T : ComponentDialog => (T)this.CreateSurvey(survey);

        private ComponentDialog CreateStep(ISurveyStepDefinition stepDefinition)
        {
            try
            {
                var type = stepDefinition.GetType();
                var stepBuilder = this.stepBuilders.First(s => s.Matches(stepDefinition));

                var step = stepBuilder.Create(stepDefinition);

                return step;
            }
            catch (Exception ex)
            {
                throw new DialogFactoryException($"Could not create ComponentDialog : Unsupported type [{stepDefinition.GetType().FullName}]");
            }
        }

        private ComponentDialog CreateSurvey(ISurvey survey)
        {
            return this.CreateLinearSurveyDialog(survey);
        }

        private SurveyDialog CreateLinearSurveyDialog(ISurvey survey)
        {
            var dialogs = new List<Dialog>();
            foreach (var step in survey.Steps)
            {
                // register each step with the dialogs stack
                dialogs.Add(this.CreateStep(step));
            }

            return new SurveyDialog(survey.Id)
                .WithSteps(survey.Steps)
                .Build(this);
        }
    }
}