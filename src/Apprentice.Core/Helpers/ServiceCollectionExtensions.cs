using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Core.Helpers
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterAllTypes<T>(this IServiceCollection services, Assembly[] assemblies, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            var typesFromAssemblies = assemblies.SelectMany(a => a.DefinedTypes.Where(x => x.GetInterfaces().Contains(typeof(T))));
            foreach (var type in typesFromAssemblies)
            {
                services.Add(new ServiceDescriptor(typeof(T), type, lifetime));
            }

            return services;
        }
    }
}
