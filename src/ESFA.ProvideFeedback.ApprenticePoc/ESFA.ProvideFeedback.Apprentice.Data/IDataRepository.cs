namespace ESFA.ProvideFeedback.Apprentice.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using Microsoft.Azure.Documents.Client;

    /// <summary>
    /// The DataRepository interface.
    /// </summary>
    public interface IDataRepository
    {
        Task<IEnumerable<T>> GetAllItemsAsync<T>(FeedOptions feedOptions = null)
            where T : TypedDocument<T>;

        Task<T> GetItemAsync<T>(Expression<Func<T, bool>> predicate, FeedOptions feedOptions = null)
            where T : TypedDocument<T>;

        Task<T> GetItemAsync<T>(string documentId, RequestOptions requestOptions = null)
            where T : TypedDocument<T>;

        Task<IEnumerable<T>> GetItemsAsync<T>(Expression<Func<T, bool>> predicate, FeedOptions feedOptions = null)
            where T : TypedDocument<T>;

        Task<bool> RemoveItemAsync<T>(string documentId, RequestOptions requestOptions = null)
            where T : TypedDocument<T>;

        Task<bool> RemoveItemAsync<T>(T document, RequestOptions requestOptions = null)
            where T : TypedDocument<T>;

        Task<bool> RemoveItemsAsync<T>(IEnumerable<T> documents, RequestOptions requestOptions = null)
            where T : TypedDocument<T>;

        Task<T> UpsertItemAsync<T>(T document, RequestOptions requestOptions = null)
            where T : TypedDocument<T>;
    }
}