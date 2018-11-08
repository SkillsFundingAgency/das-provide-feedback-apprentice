﻿using System;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.DependecyInjection
{
    /// <summary>
    /// Defines the interface of builder that creates an instance of an <see cref="IServiceProvider"/>.
    /// </summary>
    public interface IServiceProviderBuilder
    {
        /// <summary>
        /// Creates an instance of an <see cref="IServiceProvider"/>.
        /// </summary>
        /// <returns></returns>
        IServiceProvider Build();
    }
}
