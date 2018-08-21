// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationExtensions.cs" company="Education & Skills Funding Agency">
//   Copyright © 2018 Education & Skills Funding Agency
// </copyright>
// <summary>
//   Defines a set of extensions used to make configuration and settings retrieval easier.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ESFA.ProvideFeedback.Apprentice.Bot.Helpers
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Defines a set of extensions used to make configuration and settings retrieval easier.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        ///     Convention based settings registration.
        /// </summary>
        /// <typeparam name="T">The config setting type. Uses the name of the type to match it to the config file settings node</typeparam>
        /// <param name="serviceCollection">the collection of registered services</param>
        /// <param name="configuration">the configuration provider</param>
        /// <returns>the <see cref="IServiceCollection"/> </returns>
        public static IServiceCollection BindConfiguration<T>(this IServiceCollection serviceCollection, IConfiguration configuration)
            where T : class, new()
        {
            T settings = new T();
            configuration.Bind(typeof(T).Name, settings);
            serviceCollection.Configure<T>(configuration.GetSection(typeof(T).Name));
            serviceCollection.AddSingleton(settings);
            return serviceCollection;
        }
    }
}