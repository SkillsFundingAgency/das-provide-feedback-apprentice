namespace ESFA.DAS.ProvideFeedback.Apprentice.Core.Interfaces
{
    using System;

    // marker interface for DI
    public interface IQuery
    {
    }

    public interface IQuery<in TCriteria, out TResult> : IQuery, IExecutable<TCriteria, TResult>
    {
    }
}

