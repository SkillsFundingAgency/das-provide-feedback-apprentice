using Microsoft.Extensions.DependencyInjection;
using System;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.Services
{
    public class FunctionOptions
    {
        internal Func<IServiceProvider, IDistributedLockProvider> LockFactory;
        public IServiceCollection Services { get; private set; }

        public FunctionOptions(IServiceCollection services)
        {
            Services = services;
            LockFactory = new Func<IServiceProvider, IDistributedLockProvider>(sp => new NullLockProvider());
        }

        public void UseDistributedLockManager(Func<IServiceProvider, IDistributedLockProvider> factory)
        {
            LockFactory = factory;
        }
    }
}
