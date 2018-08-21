// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TurnContextExtensions.cs" company="Education & Skills Funding Agency">
//   Copyright © 2018 Education & Skills Funding Agency
// </copyright>
// <summary>
//   A set of extensions that can be used on dialog turns.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ESFA.ProvideFeedback.Apprentice.Bot.Helpers
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
        public static async Task AddRealisticTypingDelay(this ITurnContext ctx, string textToType, int charactersPerMinute, int thinkingTimeDelay)
        {
            Activity typing = new Activity() { Type = ActivityTypes.Typing };
            await ctx.SendActivity(typing);
            await Task.Delay(FormHelper.CalculateTypingTime(textToType, charactersPerMinute, thinkingTimeDelay));
        }
    }
}