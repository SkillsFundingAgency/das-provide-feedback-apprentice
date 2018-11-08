using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.DependecyInjection.Config
{
    internal class ServiceProviderBuilder : IServiceProviderBuilder
    {
        private readonly Action<IServiceCollection> _configureServices;

        public ServiceProviderBuilder(Action<IServiceCollection> configureServices) =>
            _configureServices = configureServices;

        public IServiceProvider Build()
        {
            var services = new ServiceCollection();
            _configureServices(services);
            return services.BuildServiceProvider();
        }
    }
}
