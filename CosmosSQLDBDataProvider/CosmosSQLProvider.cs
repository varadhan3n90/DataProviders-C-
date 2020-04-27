// <copyright file="CosmosSQLProvider.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DataAccessService.CosmosSql
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// CosmosSqlProvider.
    /// </summary>
    public sealed class CosmosSqlProvider : IDataProvider, IDisposable
    {
        private readonly DocumentClient _client;
        private readonly string _cosmosQueryOptionNull = "Cosmos query options is null";

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosSqlProvider"/> class.
        /// </summary>
        /// <param name="options">Connection options.</param>
        public CosmosSqlProvider(IOptions<CosmosSqlProviderOptions> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _client = new DocumentClient(options.Value.EndpointUri, options.Value.Key);
        }

        /// <summary>
        /// Gets data provider name.
        /// </summary>
        public string DataProviderName { get; } = DataProviders.CosmosSql;

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="options">Options to delete.</param>
        /// <returns>Task.</returns>
        public Task DeleteData(object options)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Used to get data from cosmos db storage.
        /// </summary>
        /// <typeparam name="T">The class object expected as output.</typeparam>
        /// <param name="queryOptions"><see cref="CosmosQueryOptions"/>.</param>
        /// <param name="dataPropertyMapper">Call back function to map raw object to required output type.</param>
        /// <returns>Data mapped by data property mapper.</returns>
        public async Task<T> GetData<T>(object queryOptions, Action<T, object> dataPropertyMapper)
            where T : new()
        {
            if (!(queryOptions is CosmosQueryOptions query))
            {
                throw new Exception(_cosmosQueryOptionNull);
            }

            var t = Activator.CreateInstance<T>();
            object dbReturnValue = null;
            if (!string.IsNullOrEmpty(query.DocumentId))
            {
                dbReturnValue = await _client.ReadDocumentAsync(
                    UriFactory.CreateDocumentUri(query.DataBaseName, query.CollectionName, query.DocumentId), new RequestOptions { PartitionKey = new PartitionKey(query.DocumentId) }).ConfigureAwait(false);
            }
            else if (!string.IsNullOrEmpty(query.Query))
            {
                var feedOptions = new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true };
                dbReturnValue = _client.CreateDocumentQuery<Document>(UriFactory.CreateDocumentCollectionUri(query.DataBaseName, query.CollectionName), query.Query, feedOptions).AsEnumerable();
            }

            if (dbReturnValue == null)
            {
                return default;
            }

            dataPropertyMapper?.Invoke(t, dbReturnValue);
            return t;
        }

        /// <summary>
        /// Function to store in cosmos.
        /// </summary>
        /// <typeparam name="T">The class data that will be stored.</typeparam>
        /// <param name="options"><see cref="CosmosQueryOptions"/> Options required to store blob.</param>
        /// <param name="t">Object that will be serialized and stored.</param>
        /// <returns>The object T that is stored.</returns>
        public Task<T> StoreData<T>(object options, T t)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Used to update existing document.
        /// </summary>
        /// <typeparam name="T">The class data that will be stored.</typeparam>
        /// <param name="options"><see cref="CosmosQueryOptions"/> Options required to store blob.</param>
        /// <param name="t">Object that will be serialized and stored.</param>
        /// <returns>The object T that is updated.</returns>
        public Task<T> UpdateData<T>(object options, T t)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Dispose the object.
        /// </summary>
        /// <param name="d">Dispose.</param>
        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
