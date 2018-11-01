using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.DependecyInjection.Services;
using Microsoft.Azure.WebJobs.Host.Bindings;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.DependecyInjection.Bindings
{
    internal class InjectBindingProvider : IBindingProvider
    {
        private readonly ServiceProviderHolder _serviceProviderHolder;

        public InjectBindingProvider(ServiceProviderHolder serviceProviderHolder) =>
            _serviceProviderHolder = serviceProviderHolder;

        public Task<IBinding> TryCreateAsync(BindingProviderContext context)
        {
            IBinding binding = new InjectBinding(_serviceProviderHolder, context.Parameter.ParameterType);
            return Task.FromResult(binding);
        }
    }
}
