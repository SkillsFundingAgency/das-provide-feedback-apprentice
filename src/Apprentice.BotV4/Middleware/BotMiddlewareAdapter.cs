namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Middleware
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Bot.Builder;
    using Microsoft.Extensions.DependencyInjection;

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
        /// <param name="turnContext">the Turn Context</param>
        /// <param name="next">the next middleware OnTurn processor</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The <see cref="T:System.Threading.Tasks.Task" /></returns>
        public Task OnTurnAsync(
            ITurnContext turnContext,
            NextDelegate next,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return this.middleware.Value.OnTurnAsync(turnContext, next, cancellationToken);
        }
    }
}