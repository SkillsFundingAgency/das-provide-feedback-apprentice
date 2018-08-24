// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApprenticeFeedbackV3.cs" company="Education & Skills Funding Agency">
//   Copyright © 2018 Education & Skills Funding Agency
// </copyright>
// <summary>
//   Represents Version 3 of the Apprentice Feedback Questionnaire
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.State;

    using Microsoft.Bot.Builder.Core.Extensions;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Prompts;
    using Microsoft.Bot.Schema;
    using Microsoft.Recognizers.Text;

    using ChoicePrompt = Microsoft.Bot.Builder.Dialogs.ChoicePrompt;
    using TextPrompt = Microsoft.Bot.Builder.Dialogs.TextPrompt;

    public class ApprenticeFeedbackV3 : DialogContainer
    {
        public const string Id = "apprentice-feedback-v3";

        private const string StateKey = nameof(ApprenticeFeedbackV3);

        private ApprenticeFeedbackV3()
            : base(Id)
        {
            // Define the conversation flow using a waterfall model.
            var steps = new WaterfallStep[]
                            {
                                async (dc, args, next) => { await StepA(dc); },
                                async (dc, args, next) => { await StepB(dc, args); },
                                async (dc, args, next) => { await StepC(dc, args); },
                                async (dc, args, next) => { await SurveyEnd(dc, args); }
                            };

            this.Dialogs.Add(Id, steps);

            // Define the prompts used in this conversation flow.
            this.Dialogs.Add("textPrompt", new TextPrompt());
            this.Dialogs.Add("multiChoicePrompt", new ChoicePrompt(Culture.English) { Style = ListStyle.None });
        }

        public static ApprenticeFeedbackV3 Instance { get; } = new ApprenticeFeedbackV3();

        private static async Task StepA(DialogContext dc)
        {
            UserInfo userInfo = UserState<UserInfo>.Get(dc.Context);
            ConversationInfo conversationInfo = ConversationState<ConversationInfo>.Get(dc.Context);

            userInfo.ApprenticeFeedback.StartDate = DateTime.Now;
            userInfo.ApprenticeFeedback.Progress = ProgressState.NotStarted;

            userInfo.ApprenticeFeedback.SurveyId = Id;

            await dc.Context.SendActivity(
                "Here’s your quarterly apprenticeship survey. You agreed to participate when you started your apprenticeship",
                inputHint: InputHints.IgnoringInput);

            await dc.Context.SendActivity(
                "It's just 3 questions and it'll really help us improve things. But if you want to opt out, please type ‘STOP’",
                inputHint: InputHints.IgnoringInput);

            // Ask question 1
            await dc.AskPolarQuestion(
                "Over the last 6 months, have you received at least 25 days of training? Please type ‘YES’ or ‘NO’");
        }

        private static async Task StepB(DialogContext dc, IDictionary<string, object> args)
        {
            UserInfo userInfo = UserState<UserInfo>.Get(dc.Context);
            ConversationInfo conversationInfo = ConversationState<ConversationInfo>.Get(dc.Context);

            userInfo.ApprenticeFeedback.Progress = ProgressState.Enagaged;

            PolarQuestionResponse response = await dc.GetPolarQuestionResponse(
                                                 args,
                                                 "Over the last 6 months, have you received at least 25 days of training? Please type ‘YES’ or ‘YES’");

            userInfo.ApprenticeFeedback.Responses.Add(response);

            if (response.IsPositive)
            {
                await dc.Context.SendActivity("Thanks!");
            }
            else
            {
                await dc.Context.SendActivity("Okay, thanks for the feedback");
            }

            await dc.AskPolarQuestion("Next question, is your trainer good?");
        }

        private static async Task StepC(DialogContext dc, IDictionary<string, object> args)
        {
            UserInfo userInfo = UserState<UserInfo>.Get(dc.Context);
            ConversationInfo conversationInfo = ConversationState<ConversationInfo>.Get(dc.Context);

            userInfo.ApprenticeFeedback.Progress = ProgressState.Enagaged;

            PolarQuestionResponse response = await dc.GetPolarQuestionResponse(
                                                 args,
                                                 "Next question, is your trainer good?");

            userInfo.ApprenticeFeedback.Responses.Add(response);

            if (response.IsPositive)
            {
                await dc.Context.SendActivity("Thanks");
            }
            else
            {
                await dc.Context.SendActivity("Okay, thanks for that");
            }

            await dc.AskPolarQuestion("Overall, are you satisfied with your apprenticeship?");
        }

        private static async Task SurveyEnd(DialogContext dc, IDictionary<string, object> args)
        {
            UserInfo userInfo = UserState<UserInfo>.Get(dc.Context);
            ConversationInfo conversationInfo = ConversationState<ConversationInfo>.Get(dc.Context);

            userInfo.ApprenticeFeedback.Progress = ProgressState.Enagaged;

            PolarQuestionResponse response = await dc.GetPolarQuestionResponse(
                                                 args,
                                                 "Overall, are you satisfied with your apprenticeship?");

            userInfo.ApprenticeFeedback.Responses.Add(response);

            if (response.IsPositive)
            {
                await dc.Context.SendActivity("Great, thanks for your feedback. It’s really helpful");
            }
            else
            {
                await dc.Context.SendActivity("Okay, sorry to hear that");
            }

            int maxScore = userInfo.ApprenticeFeedback.Responses.Count;

            if (userInfo.ApprenticeFeedback.Score >= maxScore)
            {
                await dc.Context.SendActivity("Keep up the good work!");
            }
            else
            {
                await dc.Context.SendActivity(
                    "If you have a problem with your apprenticeship, it’s a good idea to speak to your employer’s ‘Human Resources’ staff");
            }

            if (userInfo.ApprenticeFeedback.Score < 0)
            {
                await dc.Context.SendActivity(
                    "If you’ve talked to them already, you might want to make a formal complaint: https://www.gov.uk/complainfurthereducationapprenticeship");
            }

            userInfo.ApprenticeFeedback.Progress = ProgressState.Complete;
            userInfo.ApprenticeFeedback.EndDate = DateTime.Now;

            await dc.End();
        }
    }
}