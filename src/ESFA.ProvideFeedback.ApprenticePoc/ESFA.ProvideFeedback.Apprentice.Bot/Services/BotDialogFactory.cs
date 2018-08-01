using System.Threading.Tasks;
using ESFA.ProvideFeedback.Apprentice.Bot.Helpers;
using ESFA.ProvideFeedback.Apprentice.Bot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Prompts;
using Microsoft.Bot.Builder.Prompts.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text;

namespace ESFA.ProvideFeedback.Apprentice.Bot.Services
{
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

                        foreach (var r in nextStep.Responses)
                        {
                            await dc.Context.AddRealisticTypingDelay(r);
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

        public DialogSet BuildBranchingDialog(DialogSet dialogs, string dialogName, string prompt,
            IDialogStep positiveBranch, IDialogStep negativeBranch)
        {
            dialogs.Add(dialogName, new WaterfallStep[]
            {
                async (dc, args, next) =>
                {
                    await dc.Context.AddRealisticTypingDelay(prompt);
                    await dc.Prompt("confirmationPrompt", prompt,
                        FormHelper.ConfirmationPromptOptions);
                },
                async (dc, args, next) =>
                {
                    var state = ConversationState<SurveyState>.Get(dc.Context);
                    var userState = UserState<UserState>.Get(dc.Context);

                    var positive = args["Value"] is FoundChoice response && response.Value == "yes";
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

                    _logger.LogDebug(
                        $"{userState.UserName} has a survey score of {state.SurveyScore} which has triggered the {(positive ? "positive" : "negative")} conversation tree");

                    await dc.End(dc.ActiveDialog.State);
                    foreach (var r in activeBranch.Responses)
                    {
                        await dc.Context.SendActivity(r, inputHint: InputHints.IgnoringInput);
                        await dc.Context.AddRealisticTypingDelay(r);
                    }

                    await dc.Begin(activeBranch.DialogTarget);
                },
                async (dc, args, next) => { await dc.End(); }
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
                    await dc.Context.AddRealisticTypingDelay(prompt);
                    await dc.Prompt("freeText", prompt);
                },
                async (dc, args, next) =>
                {
                    await dc.Context.SendActivity("", inputHint: InputHints.IgnoringInput);

                    var state = ConversationState<SurveyState>.Get(dc.Context);
                    var userState = UserState<UserState>.Get(dc.Context);

                    var response = args["Text"];
                    _logger.LogDebug($"{userState.UserName} has wrote {response}");

                    foreach (var r in nextStep.Responses)
                    {
                        await dc.Context.SendActivity(r, inputHint: InputHints.IgnoringInput);
                        await dc.Context.AddRealisticTypingDelay(r);
                    }

                    await dc.Begin(nextStep.DialogTarget);
                },
                async (dc, args, next) => { await dc.End(); }
            });

            return dialogs;
        }

        public DialogSet BuildDynamicEndDialog(DialogSet dialogs, string dialogName, int requiredScore,
            IDialogStep positiveEnd,
            IDialogStep negativeEnd)
        {
            dialogs.Add(dialogName,
                new WaterfallStep[]
                {
                    async (dc, args, next) =>
                    {
                        var state = ConversationState<SurveyState>.Get(dc.Context);
                        var userState = UserState<UserState>.Get(dc.Context);

                        var positive = state.SurveyScore >= requiredScore;

                        // End the convo, deciding whether to use the positive or negative journey based on the user score
                        var activeEnd = positive ? positiveEnd : negativeEnd;

                        _logger.LogDebug(
                            $"{userState.UserName} has a survey score of {state.SurveyScore} which has triggered the {(positive ? "positive" : "negative")} ending");

                        foreach (var r in activeEnd.Responses)
                        {
                            await dc.Context.AddRealisticTypingDelay(r);
                            await dc.Context.SendActivity(r, inputHint: InputHints.IgnoringInput);
                        }

                        await dc.End(state);
                    }
                });

            return dialogs;
        }

        public DialogSet BuildChoicePrompt(DialogSet dialogs, string promptName, ListStyle style)
        {
            dialogs.Add(promptName,
                new Microsoft.Bot.Builder.Dialogs.ChoicePrompt(Culture.English) {Style = ListStyle.None});
            return dialogs;
        }

        public DialogSet BuildTextPrompt(DialogSet dialogs, string promptName)
        {
            dialogs.Add(promptName, new Microsoft.Bot.Builder.Dialogs.TextPrompt());
            return dialogs;
        }
    }

    public static class TurnContextExtensions
    {
        public static async Task AddRealisticTypingDelay(this ITurnContext ctx, string textToType)
        {
            Activity typing = new Activity() { Type = ActivityTypes.Typing };
            await ctx.SendActivity(typing);
            await Task.Delay(FormHelper.CalculateTypingTimeInMs(textToType));
        }
    }
}