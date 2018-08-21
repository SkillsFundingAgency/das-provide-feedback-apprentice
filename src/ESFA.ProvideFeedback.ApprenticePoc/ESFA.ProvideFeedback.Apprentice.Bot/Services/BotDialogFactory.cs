// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BotDialogFactory.cs" company="Education & Skills Funding Agency">
//   Copyright © 2018 Education & Skills Funding Agency
// </copyright>
// <summary>
//   Factory for adding conversational dialogs to Bots
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ESFA.ProvideFeedback.Apprentice.Bot.Services
{
    using ESFA.ProvideFeedback.Apprentice.Bot.Dto;
    using ESFA.ProvideFeedback.Apprentice.Bot.Helpers;
    using ESFA.ProvideFeedback.Apprentice.Bot.Models;

    using Microsoft.Bot.Builder.Core.Extensions;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Prompts;
    using Microsoft.Bot.Builder.Prompts.Choices;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Recognizers.Text;

    using BotConfig = ESFA.ProvideFeedback.Apprentice.Bot.Config.Bot;

    /// <inheritdoc />
    /// <summary>
    /// Factory for adding conversational dialogs to Bots
    /// </summary>
    public class BotDialogFactory : IDialogFactory<DialogSet>
    {
        /// <summary>
        /// The log provider
        /// </summary>
        private readonly ILogger<BotDialogFactory> logger;

        /// <summary>
        /// The configuration provider
        /// </summary>
        private readonly BotConfig botConfiguration;


        /// <summary>
        ///     Initializes a new instance of the <see cref="BotDialogFactory"/> class.
        /// </summary>
        /// <param name="log"> The log provider. </param>
        /// <param name="botConfig">the bot configuration object</param>
        public BotDialogFactory(ILogger<BotDialogFactory> log, BotConfig botConfig)
        {
            this.botConfiguration = botConfig;
            this.logger = log;
        }

        /// <summary>
        /// Gets the typing speed.
        /// </summary>
        protected int TypingSpeed => botConfiguration.Typing.CharactersPerMinute;

        /// <summary>
        /// Gets the typing delay.
        /// </summary>
        protected int TypingDelay => botConfiguration.Typing.ThinkingTimeDelay;


        /// <inheritdoc />
        public DialogSet Conversation()
        {
            return new DialogSet();
        }

        /// <inheritdoc />
        public DialogSet BuildBranchingDialog(
            DialogSet dialogs,
            string dialogName,
            string prompt,
            IDialogStep positiveBranch,
            IDialogStep negativeBranch)
        {
            dialogs.Add(
                dialogName,
                new WaterfallStep[]
                    {
                        async (dc, args, next) =>
                            {
                                await dc.Context.AddRealisticTypingDelay(prompt, this.TypingSpeed, this.TypingDelay);
                                await dc.Prompt(
                                    "confirmationPrompt",
                                    prompt,
                                    FormHelper.ConfirmationPromptOptions);
                            },
                        async (dc, args, next) =>
                            {
                                SurveyState state = dc.Context.GetConversationState<SurveyState>();
                                UserState userState = dc.Context.GetUserState<UserState>();

                                bool positive =
                                    args["Value"] is FoundChoice response && response.Value == "yes";
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

                                this.logger.LogDebug(
                                    $"{userState.UserName} has a survey score of {state.SurveyScore} which has triggered the {(positive ? "positive" : "negative")} conversation tree");

                                await dc.End(dc.ActiveDialog.State);
                                foreach (var r in activeBranch.Responses)
                                {
                                    await dc.Context.SendActivity(
                                        r,
                                        inputHint: InputHints.IgnoringInput);
                                    await dc.Context.AddRealisticTypingDelay(r, this.TypingSpeed, this.TypingDelay);
                                }

                                await dc.Begin(activeBranch.DialogTarget);
                            },
                        async (dc, args, next) => { await dc.End(); }
                    });

            return dialogs;
        }

        /// <inheritdoc />
        public DialogSet BuildChoicePrompt(DialogSet dialogs, string promptName, ListStyle style)
        {
            dialogs.Add(
                promptName,
                new Microsoft.Bot.Builder.Dialogs.ChoicePrompt(Culture.English) { Style = ListStyle.None });
            return dialogs;
        }

        /// <inheritdoc />
        public DialogSet BuildDynamicEndDialog(
            DialogSet dialogs,
            string dialogName,
            int requiredScore,
            IDialogStep positiveEnd,
            IDialogStep negativeEnd)
        {
            dialogs.Add(
                dialogName,
                new WaterfallStep[]
                    {
                        async (dc, args, next) =>
                            {
                                SurveyState conversationState = dc.Context.GetConversationState<SurveyState>();
                                UserState userState = dc.Context.GetUserState<UserState>();

                                bool positive = conversationState.SurveyScore >= requiredScore;

                                // End the conversation, deciding whether to use the positive or negative journey based on the user score
                                IDialogStep activeEnd = positive ? positiveEnd : negativeEnd;

                                this.logger.LogDebug(
                                    $"{userState.UserName} has a survey score of {conversationState.SurveyScore} which has triggered the {(positive ? "positive" : "negative")} ending");

                                foreach (var r in activeEnd.Responses)
                                {
                                    await dc.Context.AddRealisticTypingDelay(r, this.TypingSpeed, this.TypingDelay);
                                    await dc.Context.SendActivity(
                                        r,
                                        inputHint: InputHints.IgnoringInput);
                                }

                                await dc.End(conversationState);
                            }
                    });

            return dialogs;
        }

        /// <inheritdoc />
        public DialogSet BuildFreeTextDialog(DialogSet dialogs, string dialogName, string prompt, IDialogStep nextStep)
        {
            // A free text feedback entry prompt, with a simple echo back to the user
            dialogs.Add(
                dialogName,
                new WaterfallStep[]
                    {
                        async (dc, args, next) =>
                            {
                                await dc.Context.AddRealisticTypingDelay(prompt, this.TypingSpeed, this.TypingDelay);
                                await dc.Prompt("freeText", prompt);
                            },
                        async (dc, args, next) =>
                            {
                                SurveyState state = dc.Context.GetConversationState<SurveyState>();
                                UserState userState = dc.Context.GetUserState<UserState>();

                                object response = args["Text"];
                                this.logger.LogDebug($"{userState.UserName} has wrote {response}");

                                foreach (string r in nextStep.Responses)
                                {
                                    await dc.Context.SendActivity(
                                        r,
                                        inputHint: InputHints.IgnoringInput);
                                    await dc.Context.AddRealisticTypingDelay(r, this.TypingSpeed, this.TypingDelay);
                                }

                                await dc.Begin(nextStep.DialogTarget);
                            },
                        async (dc, args, next) => { await dc.End(); }
                    });

            return dialogs;
        }

        /// <inheritdoc />
        public DialogSet BuildTextPrompt(DialogSet dialogs, string promptName)
        {
            dialogs.Add(promptName, new Microsoft.Bot.Builder.Dialogs.TextPrompt());
            return dialogs;
        }

        /// <inheritdoc />
        public DialogSet BuildWelcomeDialog(DialogSet dialogs, string dialogName, IDialogStep nextStep)
        {
            // The main form
            dialogs.Add(
                "start",
                new WaterfallStep[]
                    {
                        async (dc, args, next) =>
                            {
                                SurveyState conversationState = dc.Context.GetConversationState<SurveyState>();
                                UserState userState = dc.Context.GetUserState<UserState>();
                                conversationState.SurveyScore = 0;

                                foreach (string r in nextStep.Responses)
                                {
                                    await dc.Context.AddRealisticTypingDelay(r, this.TypingSpeed, this.TypingDelay);
                                    await dc.Context.SendActivity(
                                        r,
                                        inputHint: InputHints.IgnoringInput);
                                }

                                await dc.Begin(nextStep.DialogTarget);
                            },
                        async (dc, args, next) => { await dc.End(); }
                    });

            return dialogs;
        }
    }
}