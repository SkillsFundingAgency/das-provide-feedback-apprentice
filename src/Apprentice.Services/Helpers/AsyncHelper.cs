﻿namespace ESFA.DAS.ProvideFeedback.Apprentice.Services.Helpers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public static class AsyncHelper
    {
        private static readonly TaskFactory TaskFactory = new TaskFactory(
            CancellationToken.None,
            TaskCreationOptions.None,
            TaskContinuationOptions.None,
            TaskScheduler.Default);

        public static TResult RunSync<TResult>(Func<Task<TResult>> func) =>
            TaskFactory.StartNew(func).Unwrap().GetAwaiter().GetResult();

        public static void RunSync(Func<Task> func) => TaskFactory.StartNew(func).Unwrap().GetAwaiter().GetResult();
    }
}