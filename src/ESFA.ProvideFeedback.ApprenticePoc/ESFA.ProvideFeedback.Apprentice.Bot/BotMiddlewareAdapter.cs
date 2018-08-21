// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BotMiddlewareAdapter.cs" company="Education & Skills Funding Agency">
//   Copyright © 2018 Education & Skills Funding Agency
// </copyright>
//   
// </copyright>
// <summary>
//   Defines the BotMiddlewareAdapter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ESFA.ProvideFeedback.Apprentice.Bot
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.Bot.Builder;
    using Microsoft.Extensions.DependencyInjection;

    /// <inheritdoc />
    /// <summary>
    /// The bot middleware adapter. Used to inject anything implementing IMiddleware into the Bot instantiation
    /// </summary>
    /// <typeparam name="TMiddleware">The middleware implementation to register</typeparam>
    public class BotMiddlewareAdapter<TMiddleware> : IMiddleware
        where TMiddleware : IMiddleware
    {
        /// <summary>
        /// The middleware to register
        /// </summary>
        private readonly Lazy<TMiddleware> middleware;

        /// <summary>
        /// Initializes a new instance of the <see cref="BotMiddlewareAdapter{TMiddleware}"/> class.
        /// </summary>
        /// <param name="services">
        /// The registered services collection.
        /// </param>
        public BotMiddlewareAdapter(IServiceCollection services)
        {
            this.middleware = new Lazy<TMiddleware>(() =>
                services.BuildServiceProvider().GetRequiredService<TMiddleware>());
        }

        /// <inheritdoc />
        /// <summary>
        /// Intercepts the middleware onTurn method
        /// </summary>
        /// <param name="context">the Turn Context</param>
        /// <param name="next">the next middleware OnTurn processor</param>
        /// <returns>The <see cref="T:System.Threading.Tasks.Task" /></returns>
        public Task OnTurn(ITurnContext context, MiddlewareSet.NextDelegate next)
        {
            return this.middleware.Value.OnTurn(context, next);
        }
    }
}