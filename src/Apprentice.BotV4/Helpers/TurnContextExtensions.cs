namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Helpers
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
        /// <param name="inputHint">the input hint to use. Possible values include acceptingInput, ignoringInput, expectingInput</param>
        /// <returns>The <see cref="Task"/></returns>
        public static async Task SendActivityWithRealisticDelay(this ITurnContext ctx, string textToType, int charactersPerMinute, int thinkingTimeDelay, string inputHint)
        {
            Activity typing = new Activity() { Type = ActivityTypes.Typing, InputHint = InputHints.IgnoringInput };
            await ctx.SendActivityAsync(typing);
            await Task.Delay(FormHelper.CalculateTypingTime(textToType, charactersPerMinute, thinkingTimeDelay));
            await ctx.SendActivityAsync(textToType, inputHint);
        }

        public static async Task SendTypingActivity(
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