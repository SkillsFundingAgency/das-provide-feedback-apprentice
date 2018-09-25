//namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs
//{
//    using System;
//    using System.Linq;
//    using System.Threading.Tasks;

//    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models;
//    using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;

//    using Microsoft.Bot.Builder.Dialogs;

//    public sealed class ChoicePrompt : Microsoft.Bot.Builder.Dialogs.ChoicePrompt
//    {
//        public ChoicePrompt(string culture, PromptValidatorEx.PromptValidator<ChoiceResult> validator = null)
//            : base(culture, validator)
//        {
//        }

//        /// <summary>
//        /// What happens when the dialog prompts the user for input?
//        /// </summary>
//        /// <param name="dc"></param>
//        /// <param name="options"></param>
//        /// <param name="isRetry"></param>
//        /// <returns></returns>
//        protected override async Task OnPromptAsync(DialogContext dc, PromptOptions options, bool isRetry)
//        {
//            UserInfo userInfo = UserState<UserInfo>.Get(dc.Context);

//            if (isRetry)
//            {
//                try
//                {
//                    if (options is ChoicePromptOptions retryOptions)
//                    {
//                        long retries = 1;
//                        var hasAttempts = dc.ActiveDialog.State.ContainsKey("Retries");

//                        if (hasAttempts)
//                        {
//                            retries = (long)dc.ActiveDialog.State["Retries"];
//                            retries++;

//                            dc.ActiveDialog.State["Retries"] = retries;
//                        }
//                        else
//                        {
//                            dc.ActiveDialog.State.Add("Retries", retries);
//                        }

//                        // Too many retries - replace the retry prompt and terminate the dialog
//                        if (retryOptions.Attempts <= retries)
//                        {
//                            options.RetryPromptString = retryOptions.TooManyAttemptsString;
//                            userInfo.SurveyState.Progress = ProgressState.BlackListed;

//                            // TODO: Terminate the conversation
//                        }

//                        // Check for dynamic retry prompts
//                        else if (retryOptions.RetryPromptsCollection.Any())
//                        {
//                            options.RetryPromptString =
//                                retryOptions.RetryPromptsCollection.Single(s => s.Key == retries).Value
//                                ?? retryOptions.RetryPromptString;
//                        }
//                    }
//                }
//                catch (Exception e)
//                {
//                    await dc.Context.TraceActivity(e.Message);
//                }
//            }

//            await base.OnPrompt(dc, options, isRetry);
//        }


//        protected override async Task<ChoiceResult> OnRecognize(DialogContext dc, PromptOptions options)
//        {
//            //ConversationInfo conversationInfo = ConversationState<ConversationInfo>.Get(dc.Context);
//            //conversationInfo.Attempts = 0;

//            return await base.OnRecognize(dc, options);
//        }
//    }

//}