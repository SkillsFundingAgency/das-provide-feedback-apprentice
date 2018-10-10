namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Helpers
{
    using System.Threading.Tasks;

    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;

    /// <summary>
    /// A set of extensions that can be used on dialog turns.
    /// </summary>
    public static class TurnContextExtensions
    {
        /// <summary>
        /// Adds a realistic typing delay to make the bot appear more natural.
        /// </summary>
        /// <param name="ctx">the turn context</param>
        /// <param name="textToType">the text response that the bot should type. Used to determine the length of the delay</param>
        /// <param name="charactersPerMinute">typing speed in characters per minute</param>
        /// <param name="thinkingTimeDelay">millisecond delay to wait before starting a response</param>
        /// <returns>The <see cref="Task"/></returns>
        public static async Task SendTypingActivityAsync(
            this ITurnContext ctx,
            string textToType,
            int charactersPerMinute = 500,
            int thinkingTimeDelay = 1000)
        {
            Activity typing = new Activity() { Type = ActivityTypes.Typing, InputHint = InputHints.IgnoringInput };
            await ctx.SendActivityAsync(typing);
            await Task.Delay(FormHelper.CalculateTypingTime(textToType, charactersPerMinute, thinkingTimeDelay));
        }
    }
}