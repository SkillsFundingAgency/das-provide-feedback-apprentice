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

namespace ESFA.ProvideFeedback.ApprenticeBot
{
    public class ApprenticeBot : IBot
    {
        static readonly string Intro = "Here's your quarterly apprenticeship survey. You agreed to participate when you started your apprenticeship";
        static readonly string YourFeedback = "Your feedback will really help us improve things. But if you want to opt out, please text STOP";
        static readonly string Question1 = "Q1. So far, are you learning as much as you expected? Please answer YES or NO";
        static readonly string Question2 = "Each month you should be getting at least 4 days of training that's not part of your job. Are you getting this?";
        static readonly string PositiveResult = "Keep up the good work!";
        static readonly string NegativeResult = "If you have a problem with your apprenticeship, it's a good idea to speak to your employer's Human Resources department";
        static readonly List<string> Responses = new List<string> { "Okay, thanks for the feedback" , "Okay, thanks for your feedback. It's really helpful" , "Thanks!" };


        private readonly DialogSet _dialogs;

        public ApprenticeBot()
        {
            _dialogs = new DialogSet();
            _dialogs.Add("feedback", new WaterfallStep[]
            {
                async (dc, args, next) =>
                {
                    await dc.Context.SendActivity(Intro);
                    await dc.Context.SendActivity(YourFeedback);
                    await dc.Prompt("questionOne", Question1);
                },
                async (dc, args, next) =>
                {
                    var convo = ConversationState<Dictionary<string,object>>.Get(dc.Context);

                    var response = args["Text"];
                    
                    await dc.Context.SendActivity($"You answered {response.ToString().ToLower()}");
                    await dc.Prompt("questionTwo", Question2);
                },
                async (dc, args, next) =>
                {
                    var convo = ConversationState<Dictionary<string,object>>.Get(dc.Context);

                    var response = args["Text"];
                    await dc.Context.SendActivity($"You answered {response.ToString().ToLower()}");
                },
            });

            // add the prompt, of type TextPrompt
            _dialogs.Add("questionOne", new Microsoft.Bot.Builder.Dialogs.TextPrompt());
            _dialogs.Add("questionTwo", new Microsoft.Bot.Builder.Dialogs.TextPrompt());
        }

        /// <inheritdoc />
        public async Task OnTurn(ITurnContext context)
        {
            var state = ConversationState<Dictionary<string, object>>.Get(context);
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
