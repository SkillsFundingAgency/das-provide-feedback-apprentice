using ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.DependecyInjection.Bindings;
using Microsoft.Azure.WebJobs.Host.Config;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.DependecyInjection.Config
{
    internal class InjectConfiguration : IExtensionConfigProvider
    {
        public readonly InjectBindingProvider _injectBindingProvider;

        public InjectConfiguration(InjectBindingProvider injectBindingProvider) =>
            _injectBindingProvider = injectBindingProvider;

        public void Initialize(ExtensionConfigContext context) => context
                .AddBindingRule<InjectAttribute>()
                .Bind(_injectBindingProvider);
    }
}
