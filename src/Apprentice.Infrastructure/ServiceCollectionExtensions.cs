namespace ESFA.DAS.ProvideFeedback.Apprentice.Infrastructure
{
    using System.Linq;
    using System.Reflection;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Service Collection extensions.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Register all types of a given type T
        /// </summary>
        /// <param name="services"> The services. </param>
        /// <param name="assemblies"> The assemblies. </param>
        /// <param name="lifetime"> The lifetime. </param>
        /// <typeparam name="T"> The type of services to resolve </typeparam>
        public static void RegisterAllTypes<T>(
            this IServiceCollection services,
            Assembly[] assemblies,
            ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            var typesFromAssemblies = assemblies.SelectMany(a => a.DefinedTypes.Where(x => x.GetInterfaces().Contains(typeof(T))));
            foreach (TypeInfo type in typesFromAssemblies)
            {
                services.Add(new ServiceDescriptor(typeof(T), type, lifetime));
            }
        }
    }
}