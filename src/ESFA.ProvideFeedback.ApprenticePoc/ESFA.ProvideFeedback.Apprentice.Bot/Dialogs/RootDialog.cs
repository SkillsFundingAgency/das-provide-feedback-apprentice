//namespace ESFA.ProvideFeedback.Apprentice.Bot.Dialogs
//{
//    using System;

//    using ESFA.ProvideFeedback.Apprentice.Bot.Helpers;
//    using ESFA.ProvideFeedback.Apprentice.Bot.Models;
//    using ESFA.ProvideFeedback.Apprentice.Bot.Services;

//    using Microsoft.Bot.Builder.Core.Extensions;
//    using Microsoft.Bot.Builder.Dialogs;
//    using Microsoft.Bot.Builder.Prompts;
//    using Microsoft.Bot.Builder.Prompts.Choices;
//    using Microsoft.Bot.Schema;
//    using Microsoft.Recognizers.Text;

//    [Serializable]
//    public class QuestionDialog : DialogContainer
//    {
//        public const string Id = "question";

//        private const string WakeUpKey = "Question";

//        private QuestionDialog()
//            : base(Id)
//        {
//            this.Dialogs.Add(
//                Id,
//                new WaterfallStep[]
//                    {
//                        async (dc, args, next) =>
//                            {
//                                await dc.Context.AddRealisticTypingDelay(prompt);
//                                await dc.Prompt(
//                                    "confirmationPrompt",
//                                    prompt,
//                                    FormHelper.ConfirmationPromptOptions);
//                            },
//                        async (dc, args, next) =>
//                            {
//                                var state = ConversationState<SurveyState>.Get(dc.Context);
//                                var userState = UserState<UserState>.Get(dc.Context);

//                                var positive =
//                                    args["Value"] is FoundChoice response && response.Value == "yes";
//                                IDialogStep activeBranch;

//                                if (positive)
//                                {
//                                    state.SurveyScore++;
//                                    activeBranch = positiveBranch;
//                                }
//                                else
//                                {
//                                    state.SurveyScore--;
//                                    activeBranch = negativeBranch;
//                                }

//                                _logger.LogDebug(
//                                    $"{userState.UserName} has a survey score of {state.SurveyScore} which has triggered the {(positive ? "positive" : "negative")} conversation tree");

//                                await dc.End(dc.ActiveDialog.State);
//                                foreach (var r in activeBranch.Responses)
//                                {
//                                    await dc.Context.SendActivity(
//                                        r,
//                                        inputHint: InputHints.IgnoringInput);
//                                    await dc.Context.AddRealisticTypingDelay(r);
//                                }

//                                await dc.Begin(activeBranch.DialogTarget);
//                            },
//                        async (dc, args, next) => { await dc.End(); }
//                    });

//            this.Dialogs.Add(
//                "confirmationPrompt",
//                new Microsoft.Bot.Builder.Dialogs.ChoicePrompt(Culture.English) { Style = ListStyle.None });
//        }

//        public static QuestionDialog Instance { get; } = new QuestionDialog();
//    }
//}