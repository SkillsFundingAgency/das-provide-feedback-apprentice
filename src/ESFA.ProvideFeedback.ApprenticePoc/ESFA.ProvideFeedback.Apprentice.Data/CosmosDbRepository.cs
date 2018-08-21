// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CosmosDbRepository.cs" company="Sohara Design Ltd">
//  This is based on the work of Adam Hockemeyer:
//   https://github.com/adamhockemeyer/Azure-Functions---CosmosDB-ResourceToken-Broker/blob/master/CosmosDBResourceTokenBroker.Shared/CosmosDBRepository.cs
// </copyright>
// <summary>
//   The cosmos db repository.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ESFA.ProvideFeedback.Apprentice.Data
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.Documents.Linq;

    /// <inheritdoc cref="IDataRepository" />
    public class CosmosDbRepository : IDataRepository
    {
        /// <summary>
        /// Sets up a new Instance on demand.
        /// </summary>
        private static readonly Lazy<CosmosDbRepository> lazyInstance =
            new Lazy<CosmosDbRepository>(() => new CosmosDbRepository());

        /// <summary>
        /// The auth key or resource token.
        /// </summary>
        private string authKeyOrResourceToken = string.Empty;

        /// <summary>
        /// The name of the CosmosDb collection.
        /// </summary>
        private string collection = string.Empty;
        
        /// <summary>
        /// The CosmosDb endpoint.
        /// </summary>
        private string cosmosEndpoint = string.Empty;

        /// <summary>
        /// The name of the CosmosDb database.
        /// </summary>
        private string database = string.Empty;

        /// <summary>
        /// The CosmosDb client.
        /// </summary>
        private DocumentClient documentClient;

        /// <summary>
        /// The partition key for the collection.
        /// </summary>
        private string partitionKey = string.Empty;

        /// <summary>
        /// Prevents a default instance of the <see cref="CosmosDbRepository"/> class from being created.
        /// </summary>
        private CosmosDbRepository()
        {
            // Additional Initialization.
        }

        /// <summary>
        /// The current instance.
        /// </summary>
        public static CosmosDbRepository Instance => lazyInstance.Value;

        /// <summary>
        /// The document collection uri.
        /// </summary>
        private Uri DocumentCollectionUri => UriFactory.CreateDocumentCollectionUri(this.database, this.collection);

        /// <summary>
        ///     Auth Key can be found in the Azure Portal under "Keys" or a ResourceToken can be used for access by user
        ///     permission.
        /// </summary>
        /// <param name="keyOrToken">
        ///     Auth key should only be used when the key is read-only,
        ///     otherwise a read-write auth keys (master) should only be used behind a middle-tier service and not stored on the
        ///     end client!
        ///     ResourceTokens can be used on the end client.
        /// </param>
        /// <returns>A CosmosDB <see cref="CosmosDbRepository"/></returns>
        public CosmosDbRepository AuthKeyOrResourceToken(string keyOrToken)
        {
            if (this.authKeyOrResourceToken.Equals(keyOrToken, StringComparison.OrdinalIgnoreCase))
            {
                return this;
            }

            this.authKeyOrResourceToken = keyOrToken;
            this.TryCreateDocumentClient();

            return this;
        }

        /// <summary>
        /// The collection.
        /// </summary>
        /// <param name="collectionName">
        /// The collection name.
        /// </param>
        /// <returns>
        /// The <see cref="CosmosDbRepository"/>.
        /// </returns>
        public CosmosDbRepository Collection(string collectionName)
        {
            this.collection = collectionName;
            return this;
        }

        /// <summary>
        ///     Parses a connection string into endpoint and auth key/token values.
        ///     CosmosDB Connection String found in the Azure Portal under "Keys".
        ///     If using a read-write key, this repository should only be used behind a middle-tier
        ///     service so the sensitive key can kept confidential.
        /// </summary>
        /// <param name="connectionString">
        ///     In the format
        ///     "AccountEndpoint=https://{cosmos-resource-name}.documents.azure.com:443/;AccountKey={Key-or-ResourceToken}.
        /// </param>
        /// <returns>A CosmosDB <see cref="CosmosDbRepository" /></returns>
        public CosmosDbRepository ConnectionString(string connectionString)
        {
            const string AccountEndpoint = "AccountEndpoint=";
            const string AccountKey = "AccountKey=";

            var components = connectionString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            if (components == null || components.Length != 2 || !connectionString.Contains(AccountKey)
                || !connectionString.Contains(AccountEndpoint))
            {
                throw new Exception(
                    "The connection string must contain \"AccountEndpoint=\" and \"AccountKey=\" separated by a semi-colon");
            }

            if (components[0].Contains(AccountEndpoint))
            {
                this.cosmosEndpoint = components[0].Replace(AccountEndpoint, string.Empty);
                this.authKeyOrResourceToken = components[1].Replace(AccountKey, string.Empty);
            }
            else
            {
                this.cosmosEndpoint = components[1].Replace(AccountEndpoint, string.Empty);
                this.authKeyOrResourceToken = components[0].Replace(AccountKey, string.Empty);
            }

            this.TryCreateDocumentClient();

            return this;
        }

        /// <summary>
        /// The database.
        /// </summary>
        /// <param name="databaseName">
        /// The database name.
        /// </param>
        /// <returns>
        /// The <see cref="CosmosDbRepository"/>.
        /// </returns>
        public CosmosDbRepository Database(string databaseName)
        {
            this.database = databaseName;
            return this;
        }

        /// <summary>
        ///     Sets the endpoint of the CosmosDB resource.  You can set either a connection string or an
        ///     <see cref="Endpoint(string)" /> and <see cref="AuthKeyOrResourceToken(string)" />.
        /// </summary>
        /// <param name="endpoint">I.E. https://{cosmos-resource-name}.documents.azure.com:443/</param>
        /// <returns>A CosmosDB <see cref="CosmosDbRepository" /></returns>
        public CosmosDbRepository Endpoint(string endpoint)
        {
            if (this.cosmosEndpoint.Equals(endpoint, StringComparison.OrdinalIgnoreCase))
            {
                return this;
            }

            this.cosmosEndpoint = endpoint;
            this.TryCreateDocumentClient();

            return this;
        }

        /// <inheritdoc>
        ///     <cref>IDataRepository.GetAllItemsAsync</cref>
        /// </inheritdoc>
        public async Task<IEnumerable<T>> GetAllItemsAsync<T>(FeedOptions feedOptions = null)
            where T : TypedDocument<T>
        {
            this.TrySetPartitionKey(ref feedOptions);

            var results = new List<T>();

            // Add the 'Type' of the document as a query filter, so documents can be filtered by a specific type.
            Expression<Func<T, bool>> typeCheck = p => p.Type == typeof(T).Name;

            var query = this.documentClient.CreateDocumentQuery<T>(this.DocumentCollectionUri, feedOptions)
                .Where(typeCheck).AsDocumentQuery();

            while (query.HasMoreResults)
            {
                var documents = await query.ExecuteNextAsync<T>();

                results.AddRange(documents);
            }

            return results.AsEnumerable();
        }

        public async Task<DocumentCollection> GetDocumentCollectionAsync()
        {
            //return await this.documentClient.ReadDocumentCollectionAsync(
            //           UriFactory.CreateDocumentCollectionUri(this.database, this.collection));

            return await this.GetOrCreateCollectionAsync();
        }

        /// <inheritdoc>
        ///     <cref>IDataRepository.GetItemAsync</cref>
        /// </inheritdoc>
        public async Task<T> GetItemAsync<T>(Expression<Func<T, bool>> predicate, FeedOptions feedOptions = null)
            where T : TypedDocument<T>
        {
            this.TrySetPartitionKey(ref feedOptions);

            // Add the 'Type' of the document as a query filter, so documents can be filtered by a specific type.
            Expression<Func<T, bool>> typeCheck = p => p.Type == typeof(T).Name;

            var query = this.documentClient.CreateDocumentQuery<T>(this.DocumentCollectionUri, feedOptions)
                .Where(typeCheck).Where(predicate).AsDocumentQuery();

            var results = await query.ExecuteNextAsync<T>();

            return results?.FirstOrDefault();
        }

        /// <inheritdoc>
        ///     <cref>IDataRepository.GetItemAsync</cref>
        /// </inheritdoc>
        public async Task<T> GetItemAsync<T>(string documentId, RequestOptions requestOptions = null)
            where T : TypedDocument<T>
        {
            this.TrySetPartitionKey(ref requestOptions);

            return await this.documentClient.ReadDocumentAsync<T>(
                       UriFactory.CreateDocumentUri(this.database, this.collection, documentId),
                       requestOptions);
        }

        /// <inheritdoc>
        ///     <cref>IDataRepository.GetItemsAsync</cref>
        /// </inheritdoc>
        public async Task<IEnumerable<T>> GetItemsAsync<T>(
            Expression<Func<T, bool>> predicate,
            FeedOptions feedOptions = null)
            where T : TypedDocument<T>
        {
            this.TrySetPartitionKey(ref feedOptions);

            var results = new List<T>();

            // Add the 'Type' of the document as a query filter, so documents can be filtered by a specific type.
            Expression<Func<T, bool>> typeCheck = p => p.Type == typeof(T).Name;

            var query = this.documentClient.CreateDocumentQuery<T>(this.DocumentCollectionUri, feedOptions)
                .Where(typeCheck).Where(predicate).AsDocumentQuery();

            while (query.HasMoreResults)
            {
                var documents = await query.ExecuteNextAsync<T>();

                results.AddRange(documents);
            }

            return results.AsEnumerable();
        }

        public CosmosDbRepository PartitionKey(string partitionKey)
        {
            this.partitionKey = partitionKey;
            return this;
        }

        /// <inheritdoc>
        ///     <cref>IDataRepository.RemoveItemAsync</cref>
        /// </inheritdoc>
        public async Task<bool> RemoveItemAsync<T>(string documentId, RequestOptions requestOptions = null)
            where T : TypedDocument<T>
        {
            this.TrySetPartitionKey(ref requestOptions);

            try
            {
                await this.documentClient.DeleteDocumentAsync(
                    UriFactory.CreateDocumentUri(this.database, this.collection, documentId),
                    requestOptions);
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Unable to delete document. {e.Message}");
                return false;
            }
        }

        /// <inheritdoc>
        ///     <cref>IDataRepository.RemoveItemAsync</cref>
        /// </inheritdoc>
        public async Task<bool> RemoveItemAsync<T>(T document, RequestOptions requestOptions = null)
            where T : TypedDocument<T>
        {
            this.TrySetPartitionKey(ref requestOptions);

            return await this.RemoveItemAsync<T>(document.Id, requestOptions);
        }

        /// <inheritdoc>
        ///     <cref>IDataRepository.RemoveItemAsync</cref>
        /// </inheritdoc>
        public async Task<bool> RemoveItemsAsync<T>(IEnumerable<T> documents, RequestOptions requestOptions = null)
            where T : TypedDocument<T>
        {
            this.TrySetPartitionKey(ref requestOptions);

            // TODO: Change to sproc.
            // Check for existing bulk delete sproc, if not exist, create, and execute against 'documents' property.

            // Hack for now.
            try
            {
                await Task.Run(
                    () =>
                        {
                            Parallel.ForEach(
                                documents,
                                d =>
                                    {
                                        bool result = this.RemoveItemAsync(d, requestOptions).Result;
                                    });
                        });

                return true;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <inheritdoc>
        ///     <cref>IDataRepository.UpsertItemAsync</cref>
        /// </inheritdoc>
        public async Task<T> UpsertItemAsync<T>(T document, RequestOptions requestOptions = null)
            where T : TypedDocument<T>
        {
            this.TrySetPartitionKey(ref document);

            var response = await this.documentClient.UpsertDocumentAsync(
                               this.DocumentCollectionUri,
                               document,
                               requestOptions);

            return (dynamic)response.Resource;
        }

        /// <summary>
        ///     Creates the document client based on the current config values that have been assigned by the Fluent API.
        /// </summary>
        private void TryCreateDocumentClient()
        {
            if (string.IsNullOrEmpty(this.cosmosEndpoint) || string.IsNullOrEmpty(this.authKeyOrResourceToken))
            {
                return;
            }

            // Using Direct and TCP in ConnectionPolicy for better performance, as per Microsoft guidelines
            this.documentClient = new DocumentClient(
                new Uri(this.cosmosEndpoint),
                this.authKeyOrResourceToken,
                new ConnectionPolicy
                    {
                        RetryOptions =
                            new RetryOptions
                                {
                                    MaxRetryAttemptsOnThrottledRequests = 3,
                                    MaxRetryWaitTimeInSeconds = 15
                                },
                        ConnectionMode = ConnectionMode.Direct,
                        ConnectionProtocol = Protocol.Tcp
                    });

            Debug.WriteLine($"Creating DocumentClient...");
        }

        private void TrySetPartitionKey(ref FeedOptions options)
        {
            if (string.IsNullOrEmpty(this.partitionKey))
            {
                return;
            }

            if (options == null)
            {
                options = new FeedOptions { PartitionKey = new PartitionKey(this.partitionKey) };
            }
            else
            {
                options.PartitionKey = new PartitionKey(this.partitionKey);
            }
        }

        private void TrySetPartitionKey(ref RequestOptions options)
        {
            if (string.IsNullOrEmpty(this.partitionKey))
            {
                return;
            }

            if (options == null)
            {
                options = new RequestOptions { PartitionKey = new PartitionKey(this.partitionKey) };
            }
            else
            {
                options.PartitionKey = new PartitionKey(this.partitionKey);
            }
        }

        private void TrySetPartitionKey<T>(ref T typedDocument)
            where T : TypedDocument<T>
        {
            if (!string.IsNullOrEmpty(this.partitionKey) && typedDocument != null)
            {
                typedDocument.PartitionKey = typedDocument.PartitionKey ?? this.partitionKey;
            }
        }

        /// <summary>
        /// Create the cosmos Db database and collection
        /// </summary>
        /// <returns>The <see cref="Task"/></returns>
        private async Task<DocumentCollection> GetOrCreateCollectionAsync()
        {
            try
            {
                await this.documentClient.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(this.database));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await this.documentClient.CreateDatabaseAsync(new Database { Id = this.database });
                }
                else
                {
                    throw;
                }
            }

            try
            {
                var documentCollection = await this.documentClient.ReadDocumentCollectionAsync(
                    UriFactory.CreateDocumentCollectionUri(
                        this.database,
                        this.collection));

                return documentCollection;
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    DocumentCollection conversationCollection =
                        new DocumentCollection { Id = this.collection };
                    if (this.partitionKey != string.Empty)
                    {
                        conversationCollection.PartitionKey.Paths.Add(this.partitionKey);
                    }
                    
                    RequestOptions collectionSettings =
                        new RequestOptions { OfferThroughput = 1000 }; //TODO: pull out to parameter

                    var documentCollection = await this.documentClient.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(this.database),
                        conversationCollection,
                        collectionSettings);

                    return documentCollection;
                }
                else
                {
                    throw;
                }
            }
            
        }
    }
}