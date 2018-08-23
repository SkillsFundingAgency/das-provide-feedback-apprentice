namespace ESFA.DAS.ProvideFeedback.Apprentice.Core.Interfaces
{
    using System.Threading.Tasks;

    public interface IQueryAsync<in TCriteria, TResult> : IQuery, IExecutable<TCriteria, Task<TResult>>
    {
    }
}