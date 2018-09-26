using Microsoft.Bot.Builder;

namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Dialogs
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Recognizers.Text;

    using ChoicePrompt = Microsoft.Bot.Builder.Dialogs.ChoicePrompt;
    using TextPrompt = Microsoft.Bot.Builder.Dialogs.TextPrompt;

    public class RootDialog : ComponentDialog
    {
        public const string Id = "root-dialog";

        private RootDialog()
            : base(Id)
        {
            new WaterfallStep[]
                            {
                                async (dc, args, next) => { await Menu(dc); },
                                async (dc, args, next) => { await Start(dc); },
                                async (dc, args, next) => { await dc.End(); }
                            };
        }

        public static RootDialog Instance { get; } = new RootDialog();

        private static async Task Menu(DialogContext dc)
        {
            var menu = new List<string> { "start", "stop", "reset", "expire", "status" };
            await dc.Context.SendActivityAsync(MessageFactory.SuggestedActions(menu, "How can I help you?"));
            //await dc.Continue();
        }

        private static async Task Start(DialogContext dc)
        {
            // TODO: Add bot survey builder admin interface
            string result = dc.Context.Activity.Text.Trim().ToLowerInvariant();
            IDialog dialog = dc.Dialogs.Find(result);
            if (dialog != null)
            {
                await dc.Begin(result);
            }
            
        }
    }
}