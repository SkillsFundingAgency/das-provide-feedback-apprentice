namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Middleware
{
    using System.Collections.Generic;

    using Microsoft.Bot.Builder;
    using Microsoft.Extensions.DependencyInjection;

    public static class BotBuilderMiddlewareExtensions
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