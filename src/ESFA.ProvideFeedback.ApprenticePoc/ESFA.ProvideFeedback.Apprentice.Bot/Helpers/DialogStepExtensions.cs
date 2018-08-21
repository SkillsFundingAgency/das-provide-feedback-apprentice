// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DialogStepExtensions.cs" company="Education & Skills Funding Agency">
//   Copyright © 2018 Education & Skills Funding Agency
// </copyright>
// <summary>
//   Defines some extensions to be used for creating fluent dialog steps.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ESFA.ProvideFeedback.Apprentice.Bot.Helpers
{
    using ESFA.ProvideFeedback.Apprentice.Bot.Services;

    /// <summary>
    /// Extensions to be used for creating fluent dialog steps.
    /// </summary>
    public static class DialogStepExtensions
    {
        /// <summary>
        /// Creates a response to a dialog tree answer given by a user
        /// </summary>
        /// <param name="option">the dialog step to tie the response to</param>
        /// <param name="response">the text response to echo to the user</param>
        /// <returns>See <see cref="IDialogStep"/></returns>
        public static IDialogStep WithResponse(this IDialogStep option, string response)
        {
            option.Responses.Add(response);
            return option;
        }
    }
}