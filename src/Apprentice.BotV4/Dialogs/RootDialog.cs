namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Dialogs
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.Bot.Builder.Core.Extensions;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Prompts;
    using Microsoft.Recognizers.Text;

    using ChoicePrompt = Microsoft.Bot.Builder.Dialogs.ChoicePrompt;
    using TextPrompt = Microsoft.Bot.Builder.Dialogs.TextPrompt;

    public class RootDialog : DialogContainer
    {
        public const string Id = "root-dialog";

        private RootDialog()
            : base(Id)
        {
            var steps = new WaterfallStep[]
                            {
                                async (dc, args, next) => { await Menu(dc); },
                                async (dc, args, next) => { await TakeSurvey(dc); },
                                async (dc, args, next) => { await dc.End(); }
                            };

            this.Dialogs.Add(Id, steps);

            // Define the prompts used in this conversation flow.
            this.Dialogs.Add("textPrompt", new TextPrompt());
            this.Dialogs.Add("multiChoicePrompt", new ChoicePrompt(Culture.English) { Style = ListStyle.None });
        }

        public static RootDialog Instance { get; } = new RootDialog();

        private static async Task Menu(DialogContext dc)
        {
            var menu = new List<string> { "status", "reset", "start" };
            await dc.Context.SendActivity(MessageFactory.SuggestedActions(menu, "How can I help you?"));
        }

        private static async Task TakeSurvey(DialogContext dc)
        {
            string result = dc.Context.Activity.Text.Trim().ToLowerInvariant();

            await dc.Begin(result);
        }

        private static async Task WakeWord(DialogContext dc)
        {
            var menu = new List<string> { "status", "reset", "start" };
            await dc.Context.SendActivity(MessageFactory.SuggestedActions(menu, "How can I help you?"));
        }
    }
}