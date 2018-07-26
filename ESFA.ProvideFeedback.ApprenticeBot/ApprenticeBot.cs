using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Prompts;
using Microsoft.Bot.Builder.Prompts.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using ChoiceFactory = Microsoft.Bot.Builder.Prompts.Choices.ChoiceFactory;

namespace ESFA.ProvideFeedback.ApprenticeBot
{
    public class SurveyChoice
    {
        public bool IsPositive { get; set; }
        public string Text { get; set; }
    }

    public class ApprenticeBot : IBot
    { 
        static readonly string Intro = "Here's your quarterly apprenticeship survey. You agreed to participate when you started your apprenticeship";
        static readonly string YourFeedback = "Your feedback will really help us improve things. But if you want to opt out, please text STOP";
        static readonly string Question1 = "So far, are you learning as much as you expected? Reply yes or no.";
        static readonly string Question2 = "Each month you should be getting at least 4 days of training that's not part of your job. Are you getting this?";
        static readonly string PositiveResult = "Keep up the good work!";
        static readonly string NegativeResult = "If you have a problem with your apprenticeship, it's a good idea to speak to your employer's Human Resources department";
        static readonly List<string> Responses = new List<string> { "Okay, thanks for the feedback" , "Okay, thanks for your feedback. It's really helpful" , "Thanks!" };

        private readonly DialogSet _dialogs;

        public ApprenticeBot()
        {
            var choices = new List<Choice>()
            {
                new Choice {Action = new CardAction(text: "yes", title: "yes", value: "yes"), Value = "yes", Synonyms = new List<string>() {"yep", "yeah", "ok"}},
                new Choice {Action = new CardAction(text: "no", title: "no", value: "no"), Value = "no", Synonyms = new List<string>() {"nope", "nah", "negative"}}
            };

            var list = choices.Select(c => c.Value).ToList();

            var binaryOptions = new ChoicePromptOptions()
            {
                Choices = choices,
                RetryPromptActivity = MessageFactory.Text("Please reply YES or NO") as Activity,
            };

            _dialogs = new DialogSet();
            _dialogs.Add("feedback", new WaterfallStep[]
            {
                async (dc, args, next) =>
                {
                    await dc.Context.SendActivity(Intro);
                    await dc.Context.SendActivity(YourFeedback);

                    await dc.Prompt("questionOne", Question1, binaryOptions);
                },
                async (dc, args, next) =>
                {
                    var state = ConversationState<SurveyState>.Get(dc.Context);

                    if (args["Value"] is FoundChoice response && response.Value == "yes")
                    {
                        state.SurveyScore++;
                    }
                    else
                    {
                        state.SurveyScore--;
                    }
                    await dc.Context.SendActivity(Responses.OrderBy(s => Guid.NewGuid()).First());

                    await dc.Prompt("questionTwo", Question2, binaryOptions);
                },
                async (dc, args, next) =>
                {
                    var state = ConversationState<SurveyState>.Get(dc.Context);

                    if (args["Value"] is FoundChoice response && response.Value == "yes")
                    {
                        state.SurveyScore++;
                    }
                    else
                    {
                        state.SurveyScore--;
                    }
                    await dc.Context.SendActivity(Responses.OrderBy(s => Guid.NewGuid()).First());

                    // End the convo
                    if (state.SurveyScore > 1)
                    {
                        await dc.Context.SendActivity($"{PositiveResult}");
                    }
                    else
                    {
                        await dc.Context.SendActivity($"{NegativeResult}");
                    }

                    await dc.End();
                }
            });

            _dialogs.Add("questionOne", new Microsoft.Bot.Builder.Dialogs.ChoicePrompt(Culture.English) { Style = ListStyle.None });
            _dialogs.Add("questionTwo", new Microsoft.Bot.Builder.Dialogs.ChoicePrompt(Culture.English) { Style = ListStyle.None });
        }

        /// <inheritdoc />
        public async Task OnTurn(ITurnContext context)
        {
            var state = ConversationState<SurveyState>.Get(context);
            var dc = _dialogs.CreateContext(context, state);

            if (context.Activity.Type == ActivityTypes.Message)
            {
                await dc.Continue();

                if (!context.Responded)
                {
                    if (context.Activity.Text.ToLowerInvariant().Contains("feedback"))
                    {
                        await dc.Begin("feedback");
                    }
                    else if (context.Activity.Text.ToLowerInvariant().Contains("stop"))
                    {
                        await dc.Context.SendActivity($"Feedback cancelled");
                        await dc.End();
                    }
                    else
                    {
                        await context.SendActivity($"You said '{context.Activity.Text}'");
                    }
                }
            }
        }
    }
}
