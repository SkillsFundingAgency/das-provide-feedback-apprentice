namespace ESFA.ProvideFeedback.Apprentice.Data
{
    using System.Threading.Tasks;

    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;

    /// <summary>
    /// The DataStoreAuthRepository interface.
    /// </summary>
    public interface IDataStoreAuthRepository
    {
        /// <summary>
        /// The get document collection async.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task<DocumentCollection> GetDocumentCollectionAsync();

        /// <summary>
        /// The get permission async.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="permissionId">
        /// The permission id.
        /// </param>
        /// <param name="requestOptions">
        /// The request options.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task<Permission> GetPermissionAsync(User user, string permissionId, RequestOptions requestOptions = null);

        /// <summary>
        /// The get user async.
        /// </summary>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task<User> GetUserAsync(string userId);

        /// <summary>
        /// The remove permission async.
        /// </summary>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <param name="permissionId">
        /// The permission id.
        /// </param>
        /// <param name="requestOptions">
        /// The request options.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task<bool> RemovePermissionAsync(string userId, string permissionId, RequestOptions requestOptions = null);

        /// <summary>
        /// The upsert permission async.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="permission">
        /// The permission.
        /// </param>
        /// <param name="requestOptions">
        /// The request options.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task<Permission> UpsertPermissionAsync(User user, Permission permission, RequestOptions requestOptions = null);

        /// <summary>
        ///     Creates or Updates a <see cref="User" /> in the database.
        ///     <para />
        ///     Note: Should only be called from a mid-tier service.
        /// </summary>
        /// <param name="user">the user to update/create</param>
        /// <returns>A CosmosDB <see cref="User" /></returns>
        Task<User> UpsertUserAsync(User user);
    }
}