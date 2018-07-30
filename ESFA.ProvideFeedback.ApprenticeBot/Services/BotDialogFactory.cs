using System;
using System.Linq;
using System.Threading.Tasks;
using ESFA.ProvideFeedback.ApprenticeBot.Helpers;
using ESFA.ProvideFeedback.ApprenticeBot.Models;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Prompts.Choices;
using Microsoft.Bot.Schema;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text;
using Microsoft.Bot.Builder.Prompts;

namespace ESFA.ProvideFeedback.ApprenticeBot.Services
{
    public interface IDialogFactory<T>
    {
        T BuildApprenticeFeedbackDialog();
        T BuildApprenticeFeedbackRootDialog(T dialogs);
        T BuildApprenticeFeedbackQuestionsDialog(T dialogs);
        T BuildApprenticeFeedbackAdditionalFeedbackDialog(T dialogs);

        T BuildWelcomeDialog(T dialogs, string dialogName, IDialogStep nextStep);
        T BuildBranchingDialog(T dialogs, string dialogName, string prompt, IDialogStep positiveBranch, IDialogStep negativeBranch);

        T BuildFreeTextDialog(T dialogs, string dialogName, string prompt, IDialogStep nextStep);
        T BuildDynamicEndDialog(T dialogs, string dialogName, int requiredScore, IDialogStep positiveEnd, IDialogStep negativeEnd);
        T BuildChoicePrompt(T dialogs, string promptName, ListStyle style);
        T BuildTextPrompt(T dialogs, string promptName);
    }

    /// <summary>
    /// Factory for adding conversational dialogs to Bots
    /// </summary>
    public class BotDialogFactory : IDialogFactory<DialogSet>
    {
        private readonly ILogger<BotDialogFactory> _logger;

        public BotDialogFactory(ILogger<BotDialogFactory> log)
        {
            _logger = log;
        }

        public DialogSet BuildApprenticeFeedbackDialog()
        {
            return new DialogSet();
        }

        public DialogSet BuildWelcomeDialog(DialogSet dialogs, string dialogName, IDialogStep nextStep)
        {
            // The main form
            dialogs.Add("start",
                new WaterfallStep[]
                {
                    async (dc, args, next) =>
                    {
                        var state = ConversationState<SurveyState>.Get(dc.Context);
                        state.SurveyScore = 0;

                        foreach (string r in nextStep.Responses)
                        {
                            await Task.Delay(1000);
                            await dc.Context.SendActivity(r, inputHint: InputHints.IgnoringInput);
                        }
                        await dc.Begin(nextStep.DialogTarget);
                    },
                    async (dc, args, next) =>
                    {
                        var state = ConversationState<SurveyState>.Get(dc.Context);
                        await dc.End(state);
                    }
                }
            );

            return dialogs;
        }

        public DialogSet BuildBranchingDialog(DialogSet dialogs, string dialogName, string prompt, IDialogStep positiveBranch, IDialogStep negativeBranch)
        {
            dialogs.Add(dialogName, new WaterfallStep[]
            {
                async (dc, args, next) =>
                {
                    await Task.Delay(1000);
                    await dc.Prompt("confirmationPrompt", prompt,
                        FormHelper.ConfirmationPromptOptions);
                },
                async (dc, args, next) =>
                {
                    var state = ConversationState<SurveyState>.Get(dc.Context);

                    var response = args["Value"] as FoundChoice;
                    var positive = response.Value == "yes" ? true : false;
                    IDialogStep activeBranch;
                    
                    if (positive)
                    {
                        state.SurveyScore++;
                        activeBranch = positiveBranch;
                    }
                    else
                    {
                        state.SurveyScore--;
                        activeBranch = negativeBranch;
                    }

                    await dc.Context.SendActivity($"DEBUG: You have a survey score of {state.SurveyScore} which has triggered the {(positive ? "positive" : "negative")} conversation tree");
                    foreach (string r in activeBranch.Responses)
                    {
                        await Task.Delay(1000);
                        await dc.Context.SendActivity(r, inputHint: InputHints.IgnoringInput);
                    }
                    await dc.Begin(activeBranch.DialogTarget);
                },
                async (dc, args, next) =>
                {
                    await dc.End();
                }
            });

            return dialogs;
        }

        public DialogSet BuildFreeTextDialog(DialogSet dialogs, string dialogName, string prompt, IDialogStep nextStep)
        {
            // A free text feedback entry prompt, with a simple echo back to the user
            dialogs.Add(dialogName, new WaterfallStep[]
            {
                async (dc, args, next) =>
                {
                    await Task.Delay(1000);
                    await dc.Prompt("freeText", prompt);
                },
                async (dc, args, next) =>
                {
                    var state = ConversationState<SurveyState>.Get(dc.Context);

                    var response = args["Text"];
                    await dc.Context.SendActivity($"DEBUG: You wrote: {response}. TBC: save to database and flag if necessary.");

                    foreach (string r in nextStep.Responses)
                    {
                        await Task.Delay(1000);
                        await dc.Context.SendActivity(r, inputHint: InputHints.IgnoringInput);
                    }

                    await dc.Begin(nextStep.DialogTarget);
                },
                async (dc, args, next) =>
                {
                    await dc.End();
                }
            });

            return dialogs;
        }

        public DialogSet BuildDynamicEndDialog(DialogSet dialogs, string dialogName, int requiredScore, IDialogStep positiveEnd,
            IDialogStep negativeEnd)
        {
            dialogs.Add(dialogName,
                new WaterfallStep[]
                {
                    async (dc, args, next) =>
                    {
                        var state = ConversationState<SurveyState>.Get(dc.Context);
                        var user = ConversationState<UserState>.Get(dc.Context);

                        var positive = state.SurveyScore >= requiredScore ? true : false;
                        IDialogStep activeEnd;

                        // End the convo, deciding whether to use the positive or negative journey based on the user score
                        activeEnd = positive ? positiveEnd : negativeEnd;

                        await dc.Context.SendActivity($"DEBUG: You have a survey score of {state.SurveyScore} which has triggered the {(positive ? "positive" : "negative")} ending");
                        foreach (string r in activeEnd.Responses)
                        {
                            await Task.Delay(1000);
                            await dc.Context.SendActivity(r, inputHint: InputHints.IgnoringInput);
                        }

                        await dc.End(state);
                    }
                });

            return dialogs;
        }

        public DialogSet BuildChoicePrompt(DialogSet dialogs, string promptName, ListStyle style)
        {
            dialogs.Add(promptName, new Microsoft.Bot.Builder.Dialogs.ChoicePrompt(Culture.English) { Style = ListStyle.None });
            return dialogs;
        }

        public DialogSet BuildTextPrompt(DialogSet dialogs, string promptName)
        {
            dialogs.Add(promptName, new Microsoft.Bot.Builder.Dialogs.TextPrompt());
            return dialogs;
        }

        [Obsolete("Please build start dialogs using BuildWelcomeDialog method instead")]
        public DialogSet BuildApprenticeFeedbackRootDialog(DialogSet dialogs)
        {
            // The main form
            dialogs.Add("start",
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
                        await dc.End(state);
                    }
                }
            );

            return dialogs;
        }

        [Obsolete("Please build branching dialogs using BuildConversationBranch method instead")]
        // <see cref="BuildConversationBranch">
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
                    await dc.Begin("areYouGettingTraining");
                },
                async (dc, args, next) =>
                {
                    var state = ConversationState<SurveyState>.Get(dc.Context);
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

        [Obsolete("Please build free-text dialogs using BuildFreeTextEntry method instead")]
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