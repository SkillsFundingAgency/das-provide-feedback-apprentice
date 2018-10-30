using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs
{
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Feedback.Components;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration;

    using Microsoft.Bot.Builder.Dialogs;

    public interface IComponentBuilder<out TDialog, in TStep>
        where TDialog : ComponentDialog
        where TStep : ISurveyStep
    {
        bool IsFor(ISurveyStep step);

        TDialog Build(TStep step, FeedbackBotStateRepository state, Bot botSettings, Features features);
    }

    public class MultipleChoiceDialogComponentBuilder : IComponentBuilder<MultipleChoiceDialog, BinaryQuestion>
    {
        public bool IsFor(ISurveyStep step)
        {
            return step is BinaryQuestion;
        }

        public MultipleChoiceDialog Build(
            BinaryQuestion step,
            FeedbackBotStateRepository state,
            Bot botSettings,
            Features features)
        {
            return new MultipleChoiceDialog(step.Id, state, botSettings, features)
                .WithPrompt(step.Prompt)
                .WithResponses(step.Responses)
                .WithScore(step.Score)
                .Build();
        }
    }

    public class FreeTextDialogComponentBuilder : IComponentBuilder<FreeTextDialog, FreeTextQuestion>
    {
        public bool IsFor(ISurveyStep step)
        {
            return step is FreeTextQuestion;
        }

        public FreeTextDialog Build(FreeTextQuestion step, FeedbackBotStateRepository state, Bot botSettings, Features features)
        {
            throw new NotImplementedException();
        }
    }

    public class SurveyStartDialogComponentBuilder : IComponentBuilder<SurveyStartDialog, StartStep>
    {
        public bool IsFor(ISurveyStep step)
        {
            return step is StartStep;
        }

        public SurveyStartDialog Build(StartStep step, FeedbackBotStateRepository state, Bot botSettings, Features features)
        {
            throw new NotImplementedException();
        }
    }

    public class SurveyEndDialogComponentBuilder : IComponentBuilder<SurveyEndDialog, EndStep>
    {
        public bool IsFor(ISurveyStep step)
        {
            return step is EndStep;
        }

        public SurveyEndDialog Build(EndStep step, FeedbackBotStateRepository state, Bot botSettings, Features features)
        {
            throw new NotImplementedException();
        }
    }
}
