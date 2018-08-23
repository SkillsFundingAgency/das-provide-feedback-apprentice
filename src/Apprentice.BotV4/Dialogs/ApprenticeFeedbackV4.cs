// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApprenticeFeedbackV4.cs" company="Education & Skills Funding Agency">
//   Copyright © 2018 Education & Skills Funding Agency
// </copyright>
// <summary>
//   Represents Version 4 of the Apprentice Feedback Questionnaire
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Dialogs
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.State;

    using Microsoft.Bot.Builder.Core.Extensions;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Prompts;
    using Microsoft.Recognizers.Text;

    using ChoicePrompt = Microsoft.Bot.Builder.Dialogs.ChoicePrompt;
    using TextPrompt = Microsoft.Bot.Builder.Dialogs.TextPrompt;

    public class ApprenticeFeedbackV4 : DialogContainer
    {
        public const string Id = "apprentice-feedback-v4";

        private const string StateKey = nameof(ApprenticeFeedbackV4);

        private ApprenticeFeedbackV4()
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

        public static ApprenticeFeedbackV4 Instance { get; } = new ApprenticeFeedbackV4();

        private static async Task StepA(DialogContext dc)
        {
            // Ensure that the DialogState is clean
            await dc.BeginState<ApprenticeFeedback>(StateKey);

            // Ask question 1
            await dc.AskPolarQuestion(
                "Over the last 6 months, have you received at least 25 days of training? Please type ‘Yes’ or ‘No’");
        }

        private static async Task StepB(DialogContext dc, IDictionary<string, object> args)
        {
            ApprenticeFeedback state = 
                await dc.GetDialogState<ApprenticeFeedback>(StateKey);
            PolarQuestionResponse response = 
                await dc.GetPolarQuestionResponse(args, "Over the last 6 months, have you received at least 25 days of training? Please type ‘Yes’ or ‘No’");

            state.Responses.Add(response);

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
            ApprenticeFeedback state = 
                await dc.GetDialogState<ApprenticeFeedback>(StateKey);
            PolarQuestionResponse response = 
                await dc.GetPolarQuestionResponse(args, "Next question, is your trainer good?");

            state.Responses.Add(response);

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
            ApprenticeFeedback state =
                await dc.GetDialogState<ApprenticeFeedback>(StateKey);
            PolarQuestionResponse response =
                await dc.GetPolarQuestionResponse(args, "Overall, are you satisfied with your apprenticeship?");

            state.Responses.Add(response);

            if (response.IsPositive)
            {
                await dc.Context.SendActivity("Great, thanks for your feedback. It’s really helpful");
            }
            else
            {
                await dc.Context.SendActivity("Okay, sorry to hear that");
            }

            int maxScore = state.Responses.Count;

            if (response.Score >= maxScore)
            {
                await dc.Context.SendActivity("Keep up the good work!");
            }
            else
            {
                await dc.Context.SendActivity(
                    "If you have a problem with your apprenticeship, it’s a good idea to speak to your employer’s ‘Human Resources’ staff");
            }

            if (response.Score < 0)
            {
                await dc.Context.SendActivity(
                    "If you’ve talked to them already, you might want to make a formal complaint: https://www.gov.uk/complainfurthereducationapprenticeship");
            }

            // Save dialog state to user state and end the dialog.
            UserInfo userState = UserState<UserInfo>.Get(dc.Context);
            userState.ApprenticeFeedback = state;

            await dc.End();
        }
    }
}