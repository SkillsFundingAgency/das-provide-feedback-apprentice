using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Prompts.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace ESFA.ProvideFeedback.ApprenticeBot
{
    public interface IDialogFactory<T>
    {
        T BuildApprenticeFeedbackDialog();
        T BuildApprenticeFeedbackRootDialog(T dialogs);
        T BuildApprenticeFeedbackQuestionsDialog(T dialogs);
        T BuildApprenticeFeedbackAdditionalFeedbackDialog(T dialogs);
    }

    public static class ApprenticeFeedbackDialogExtensions
    {
        public static DialogSet WithRootDialog(this DialogSet dialogs, IDialogFactory<DialogSet> factory)
        {
            return factory.BuildApprenticeFeedbackRootDialog(dialogs);
        }
        public static DialogSet WithQuestionSet(this DialogSet dialogs, IDialogFactory<DialogSet> factory)
        {
            return factory.BuildApprenticeFeedbackQuestionsDialog(dialogs);
        }
        public static DialogSet WithAdditionalFeedback(this DialogSet dialogs, IDialogFactory<DialogSet> factory)
        {
            return factory.BuildApprenticeFeedbackAdditionalFeedbackDialog(dialogs);
        }
    }

    /// <summary>
    /// Factory for adding conversational dialogs to Bots
    /// </summary>
    public class BotDialogFactory : IDialogFactory<DialogSet>
    {
        private readonly ILogger _logger;

        internal static readonly List<string> Responses = new List<string> { "Okay, thanks for the feedback", "Okay, thanks for your feedback. It's really helpful", "Thanks!" };

        public BotDialogFactory(ILogger log)
        {
            _logger = log;
        }

        public DialogSet BuildApprenticeFeedbackDialog()
        {
            return new DialogSet();
        }

        public DialogSet BuildApprenticeFeedbackRootDialog(DialogSet dialogs)
        {
            // The main form
            dialogs.Add("firstRun",
                new WaterfallStep[]
                {
                    async (dc, args, next) =>
                    {
                        await dc.Context.SendActivity("Here's your quarterly apprenticeship survey. You agreed to participate when you started your apprenticeship", inputHint: InputHints.IgnoringInput);
                        await Task.Delay(1000);
                        await dc.Context.SendActivity("Your feedback will really help us improve things. But if you want to opt out, please text STOP", inputHint: InputHints.AcceptingInput);
                        await Task.Delay(1000);
                        await dc.Begin("apprenticeFeedbackQuestionnaire");
                    },
                    async (dc, args, next) =>
                    {
                        var state = ConversationState<SurveyState>.Get(dc.Context);
                        var user = ConversationState<UserState>.Get(dc.Context);

                        // End the convo
                        if (state.SurveyScore > 1)
                        {
                            await Task.Delay(1000);
                            _logger.LogDebug($"User {user.UserName} has a survey score of {state.SurveyScore} which has triggered the positive conversation tree");
                            //await dc.Context.SendActivity($"DEBUG: You have a survey score of {state.SurveyScore} which has triggered the negative conversation tree");
                            await dc.Context.SendActivity($"Keep up the good work!", inputHint: InputHints.IgnoringInput);
                        }
                        else
                        {
                            await Task.Delay(1000);
                            _logger.LogDebug($"User {user.UserName} has a survey score of {state.SurveyScore} which has triggered the positive conversation tree");
                            //await dc.Context.SendActivity($"DEBUG: You have a survey score of {state.SurveyScore} which has triggered the negative conversation tree");
                            await dc.Context.SendActivity($"If you have a problem with your apprenticeship, it's a good idea to speak to your employer's Human Resources department", inputHint: InputHints.IgnoringInput);
                        }

                        await dc.Begin("otherComments");
                    },
                    async (dc, args, next) =>
                    {
                        var state = ConversationState<SurveyState>.Get(dc.Context);
                        await dc.Context.SendActivity(Responses.OrderBy(s => Guid.NewGuid()).First());
                        await dc.End(state);
                    }
                }
            );

            return dialogs;
        }

        public DialogSet BuildApprenticeFeedbackQuestionsDialog(DialogSet dialogs)
        {
            // The feedback questionnaire. Invoked by typing '/feedback' to the bot, or an external trigger
            dialogs.Add("apprenticeFeedbackQuestionnaire", new WaterfallStep[]
            {
                async (dc, args, next) =>
                {
                    await dc.Begin("areYouLearning");
                },
                async (dc, args, next) =>
                {
                    await Task.Delay(1000);
                    await dc.Context.SendActivity(Responses.OrderBy(s => Guid.NewGuid()).First(), inputHint: InputHints.IgnoringInput);
                    await dc.Begin("areYouGettingTraining");
                },
                async (dc, args, next) =>
                {
                    var state = ConversationState<SurveyState>.Get(dc.Context);
                    await dc.Context.SendActivity(Responses.OrderBy(s => Guid.NewGuid()).First());
                    await dc.End(state);
                },
            });

            // A question, with associated answer handling
            dialogs.Add("areYouLearning", new WaterfallStep[]
            {
                async (dc, args, next) =>
                {
                    await Task.Delay(1000);
                    await dc.Prompt("question", "So far, are you learning as much as you expected? Reply yes or no.",
                        FormHelper.ConfirmationPromptOptions);
                },
                async (dc, args, next) =>
                {
                    var state = ConversationState<SurveyState>.Get(dc.Context);

                    var response = args["Value"] as FoundChoice;
                    var score = response.Value == "yes"
                        ? state.SurveyScore++
                        : state.SurveyScore--;

                    await dc.Context.SendActivity($"DEBUG: You answered {response.Value} which resulted in a survey score of {state.SurveyScore}");

                    await dc.End(state);
                }
            });

            // A question, with associated error handling
            dialogs.Add("areYouGettingTraining", new WaterfallStep[]
            {
                async (dc, args, next) =>
                {
                    await Task.Delay(1000);
                    await dc.Prompt("question", "Each month you should be getting at least 4 days of training that's not part of your job. Are you getting this?",
                        FormHelper.ConfirmationPromptOptions);
                },
                async (dc, args, next) =>
                {
                    var state = ConversationState<SurveyState>.Get(dc.Context);

                    var response = args["Value"] as FoundChoice;
                    var score = response.Value == "yes"
                        ? state.SurveyScore++
                        : state.SurveyScore--;

                    await dc.Context.SendActivity($"DEBUG: You answered {response.Value} which resulted in a survey score of {state.SurveyScore}");

                    await dc.End(state);
                }
            });

            return dialogs;
        }

        public DialogSet BuildApprenticeFeedbackAdditionalFeedbackDialog(DialogSet dialogs)
        {
            // A free text feedback entry prompt, with a simple echo back to the user
            dialogs.Add("otherComments", new WaterfallStep[]
            {
                async (dc, args, next) =>
                {
                    await Task.Delay(1000);
                    await dc.Prompt("freeText", "If you would like to leave any additional feedback, please reply to this message. Thanks for your time, it's very useful to us!");
                },
                async (dc, args, next) =>
                {
                    var state = ConversationState<SurveyState>.Get(dc.Context);
                    var response = args["Text"];

                    await dc.Context.SendActivity($"DEBUG: You wrote: {response}. TBC: save to database and flag if necessary.");
                    await dc.End(state);
                }
            });

            return dialogs;
        }
    }
}