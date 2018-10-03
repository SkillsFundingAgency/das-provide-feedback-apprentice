namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Helpers
{
    public static class FormHelper
    {
        /// <summary>
        /// Calculates a typing delay in MS, to make the bot responses appear more natural
        /// </summary>
        /// <param name="textToType">The string that the bot is typing</param>
        /// <param name="charactersPerMinute">typing speed in characters per minute</param>
        /// <param name="thinkingTimeDelay">millisecond delay to wait before starting a response</param>
        /// <returns>A delay in milliseconds to wait while the bot is 'typing' the response</returns>
        public static int CalculateTypingTime(string textToType, int charactersPerMinute, int thinkingTimeDelay)
        {
            if (string.IsNullOrEmpty(textToType))
            {
                return 0;
            }

            var charactersPerSecond = charactersPerMinute / 60;
            var typingDelay = textToType.Length / charactersPerSecond * 1000;
            return thinkingTimeDelay + typingDelay;
        }
    }
}