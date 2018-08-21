namespace ESFA.ProvideFeedback.Apprentice.Bot.Helpers
{
    using System.Collections.Generic;

    using Microsoft.Bot.Builder;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Extensions for registering middleware with the Bot Framework.
    /// </summary>
    public static class BotBuilderMiddlewareExtension
    {
        /// <summary>
        /// Add a new middleware to the bot framework options
        /// </summary>
        /// <typeparam name="TMiddleware">The type of middleware to add</typeparam>
        /// <param name="middleware">the list of middleware added to the bot framework</param>
        /// <param name="services">the collection of registered services</param>
        public static void Add<TMiddleware>(this IList<IMiddleware> middleware, IServiceCollection services)
            where TMiddleware : IMiddleware
        {
            middleware.Add(new BotMiddlewareAdapter<TMiddleware>(services));
        }
    }
}