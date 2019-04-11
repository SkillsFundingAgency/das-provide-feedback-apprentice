using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFunctionSupport(this IServiceCollection services, Action<FunctionOptions> setupAction = null)
        {
            if (services.Any(x => x.ServiceType == typeof(FunctionOptions)))
            {
                throw new InvalidOperationException("FunctionOptions already registered");
            }

            var options = new FunctionOptions(services);
            setupAction?.Invoke(options);

            services.AddSingleton(options.LockFactory);

            return services;
        }
    }
}
