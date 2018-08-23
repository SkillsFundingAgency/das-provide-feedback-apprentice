namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Dialogs
{
    using System.Collections.Generic;

    using Microsoft.Bot.Builder.Core.Extensions;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Prompts;
    using Microsoft.Bot.Schema;
    using Microsoft.Recognizers.Text;

    using ChoicePrompt = Microsoft.Bot.Builder.Dialogs.ChoicePrompt;
    using TextPrompt = Microsoft.Bot.Builder.Dialogs.TextPrompt;

    public class SurveyRunner : DialogContainer
    {
        public const string Id = "survey-runner";

        private SurveyRunner()
            : base(Id)
        {
            // Define the conversation flow using a waterfall model.
            var steps = new WaterfallStep[]
                            {
                                async (dc, args, next) =>
                                    {
                                        var menu = new List<string> { ApprenticeFeedbackV3.Id, ApprenticeFeedbackV4.Id };
                                        await dc.Context.SendActivity(MessageFactory.SuggestedActions(menu, "Great! Which surveyId would you like to start?"));
                                    },
                                async (dc, args, next) =>
                                    {
                                        string result = (args["Activity"] as Activity)?.Text?.Trim().ToLowerInvariant();
                                        await dc.Begin(result);
                                    },
                                async (dc, args, next) => { await dc.End(); }
                            };

            this.Dialogs.Add(Id, steps);

            // Define the prompts used in this conversation flow.
            this.Dialogs.Add("textPrompt", new TextPrompt());
            this.Dialogs.Add(ApprenticeFeedbackV3.Id, ApprenticeFeedbackV3.Instance);
            this.Dialogs.Add(ApprenticeFeedbackV4.Id, ApprenticeFeedbackV4.Instance);
        }

        public static SurveyRunner Instance { get; } = new SurveyRunner();
    }
}