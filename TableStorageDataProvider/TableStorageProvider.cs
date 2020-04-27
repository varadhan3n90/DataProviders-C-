// <copyright file="TableStorageProvider.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DataAccessService.TableStorageDataProvider
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Options;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    // ReSharper disable once ClassNeverInstantiated.Global

    /// <summary>
    /// Class to interact with table storage.
    /// </summary>
    public class TableStorageProvider : IDataProvider
    {
        private readonly CloudTableClient _cloudTableClient;
        private readonly Dictionary<string, CloudTable> _preloadedCloudTables;
        private readonly IOptions<TableStorageOptions> _tableStorageoptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableStorageProvider"/> class.
        /// </summary>
        /// <param name="options">TableStorageOptions.</param>
        public TableStorageProvider(IOptions<TableStorageOptions> options)
        {
            _tableStorageoptions = options ?? throw new ArgumentNullException(nameof(options));
            _preloadedCloudTables = new Dictionary<string, CloudTable>();
            var storageAccount = CloudStorageAccount.Parse(options.Value.StorageConnectionString);
            _cloudTableClient = storageAccount.CreateCloudTableClient();
            if (string.IsNullOrEmpty(options.Value.TablesToCreateAtStartup))
            {
                return;
            }

            foreach (var item in options.Value.TablesToCreateAtStartup.Split(';'))
            {
                var table = _cloudTableClient.GetTableReference(item);

                // Pre create so that time to create is reduced
                table.CreateIfNotExistsAsync();
                _preloadedCloudTables.Add(item, table);
            }
        }

        /// <summary>
        /// Gets DataProviderName.
        /// </summary>
        public string DataProviderName { get; } = DataProviders.AzureTableStorage;

        /// <summary>
        /// To Delete data from azure table storage.
        /// </summary>
        /// <param name="options"><see cref="TableOptions"/> options.</param>
        /// <returns>Task.</returns>
        public async Task DeleteData(object options)
        {
            if (!(options is TableOptions query))
            {
                throw new ArgumentNullException(nameof(options));
            }

            var table = GetCloudTable(query.TableName);
            if (string.IsNullOrEmpty(query.RowKey))
            {
                var items = await GetFromStorage(table, query.PartitionKey).ConfigureAwait(false);
                var batch = new TableBatchOperation();
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        if (batch.Count > 90)
                        {
                            await table.ExecuteBatchAsync(batch).ConfigureAwait(false);
                            batch = new TableBatchOperation();
                        }
                        else
                        {
                            batch.Add(TableOperation.Delete(item));
                        }
                    }
                }

                if (batch.Count > 0)
                {
                    await table.ExecuteBatchAsync(batch).ConfigureAwait(false);
                }
            }
            else
            {
                var entity = new DynamicTableEntity(query.PartitionKey, query.RowKey);
                await table.ExecuteAsync(TableOperation.Delete(entity)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="queryOptions"><see cref="TableOptions"/> or <see cref="TableStorageQueryOptions"/>.</param>
        /// <param name="dataPropertyMapper">Used to map raw data to required object.</param>
        /// <returns>Mapped object.</returns>
        public async Task<T> GetData<T>(object queryOptions, Action<T, object> dataPropertyMapper)
            where T : new()
        {
            var t = Activator.CreateInstance<T>();
            switch (queryOptions)
            {
                case TableOptions to:
                    var tbl = GetCloudTable(to.TableName);
                    var y = await GetFromStorage(tbl, to.PartitionKey, to.RowKey).ConfigureAwait(false);
                    dataPropertyMapper?.Invoke(t, y);
                    return t;
                case TableStorageQueryOptions<T> q:
                    var table = GetCloudTable(q.TableName);
                    var x = await Query(q.ProjectionQuery, table).ConfigureAwait(false);
                    dataPropertyMapper?.Invoke(t, x);
                    return t;
                default:
                    throw new ArgumentNullException(nameof(queryOptions));
            }
        }

        /// <summary>
        /// Store data in azure table storage.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="options">Options to store data.</param>
        /// <param name="t">Object to store.</param>
        /// <returns>Stored object.</returns>
        public async Task<T> StoreData<T>(object options, T t)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return await StoreDataHelper(options, t, false).ConfigureAwait(false);
        }

        /// <summary>
        /// Update data in azure table storage.
        /// </summary>
        /// <typeparam name="T">Type of object to update.</typeparam>
        /// <param name="options">Options.</param>
        /// <param name="t">Object to update.</param>
        /// <returns>Updated object.</returns>
        public async Task<T> UpdateData<T>(object options, T t)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return await StoreDataHelper(options, t, true).ConfigureAwait(false);
        }

        /// <summary>
        /// Query cloud table.
        /// </summary>
        /// <typeparam name="T">Return Type.</typeparam>
        /// <param name="query">Table Query.</param>
        /// <param name="cloudTable">Cloud table to query.</param>
        /// <returns>Results.</returns>
        private async Task<IEnumerable<T>> Query<T>(TableQuery<T> query, CloudTable cloudTable)
            where T : ITableEntity, new()
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            var entities = new List<T>();
            TableContinuationToken token = null;
            var nextQuery = query;
            do
            {
                try
                {
                    var queryResult = await cloudTable.ExecuteQuerySegmentedAsync(nextQuery, token).ConfigureAwait(false);
                    entities.AddRange(queryResult.Results);
                    token = queryResult.ContinuationToken;

                    // Continuation token is not null, more records to load.
                    if (token != null && query.TakeCount.HasValue)
                    {
                        // Query has a take count, calculate the remaining number of items to load.
                        var itemsToLoad = query.TakeCount.Value - entities.Count;

                        // If more items to load, update query take count, or else set next query to null.
                        nextQuery = itemsToLoad > 0
                            ? query.Take(itemsToLoad)
                            : null;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            while (token != null && nextQuery != null);

            return entities;
        }

        private CloudTable GetCloudTable(string tableName)
        {
            if (_preloadedCloudTables.ContainsKey(tableName))
            {
                return _preloadedCloudTables[tableName];
            }

            var table = _cloudTableClient.GetTableReference(tableName);
            table.CreateIfNotExistsAsync().ConfigureAwait(false);
            _preloadedCloudTables[tableName] = table;

            return table;
        }

        private async Task<IEnumerable<DynamicTableEntity>> GetFromStorage(CloudTable table, string partitionKey, string rowKey = null)
        {
            var filter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey);
            if (!string.IsNullOrEmpty(rowKey))
            {
                var rowFilter = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey);
                filter = TableQuery.CombineFilters(filter, TableOperators.And, rowFilter);
            }

            var query = new TableQuery<DynamicTableEntity>().Where(filter);
            var ele = await Query(query, table).ConfigureAwait(false);
            return ele;
        }

        private async Task<T> StoreDataHelper<T>(object options, T t, bool updateIfExists)
        {
            if (!(options is TableOptions query))
            {
                throw new ArgumentNullException(nameof(options));
            }

            var table = GetCloudTable(query.TableName);
            ITableEntity itTableEntity;
            if (t is IEnumerable ie)
            {
                var batchOperation = new TableBatchOperation();
                foreach (var x in ie)
                {
                    // Maximum size of batch operation can be 100 or 4MB
                    if (batchOperation.Count > 90)
                    {
                        await table.ExecuteBatchAsync(batchOperation).ConfigureAwait(false);
                        batchOperation = new TableBatchOperation();
                    }

                    if (x is ITableEntity itx)
                    {
                        if (updateIfExists)
                        {
                            batchOperation.InsertOrMerge(itx);
                        }
                        else
                        {
                            batchOperation.Insert(itx);
                        }
                    }
                    else
                    {
                        var ix = EntityConverter.ConvertToDynamicTableEntity(
                            x,
                            _ => query.PartitionKey,
                            _ => query.RowKeyGenerator != null ? query.RowKeyGenerator(x) : query.RowKey,
                            DateTime.Now,
                            "etag",
                            convertToTimeZone: _tableStorageoptions.Value.SourceTimeZone,
                            fieldsAllowedForTimeZoneConversion: (options as TableOptions).FieldsAllowedForTimeZoneConversion);
                        if (updateIfExists)
                        {
                            batchOperation.InsertOrMerge(ix);
                        }
                        else
                        {
                            batchOperation.Insert(ix);
                        }
                    }
                }

                if (batchOperation.Count > 0)
                {
                    await table.ExecuteBatchAsync(batchOperation).ConfigureAwait(false);
                }

                return t;
            }

            if (!(t is ITableEntity it))
            {
                itTableEntity = EntityConverter.ConvertToDynamicTableEntity(
                    t,
                    _ => query.PartitionKey,
                    _ => query.RowKey ?? query.RowKeyGenerator(t),
                    DateTime.Now,
                    "etag",
                    convertToTimeZone: _tableStorageoptions.Value.SourceTimeZone,
                    fieldsAllowedForTimeZoneConversion: (options as TableOptions).FieldsAllowedForTimeZoneConversion);
            }
            else
            {
                itTableEntity = it;
            }

            // Create the table if it doesn't exist.
            // await table.CreateIfNotExistsAsync();
            var de = EntityConverter.ConvertToDynamicTableEntity(
                t,
                itTableEntity.PartitionKey, // choose a partitionKey
                itTableEntity.RowKey, // choose a rowKey
                convertToTimeZone: _tableStorageoptions.Value.SourceTimeZone,
                fieldsAllowedForTimeZoneConversion: (options as TableOptions).FieldsAllowedForTimeZoneConversion);
            var tableOperation = updateIfExists ? TableOperation.InsertOrReplace(de) : TableOperation.Insert(de);
            await table.ExecuteAsync(tableOperation).ConfigureAwait(false);
            return t;
        }
    }
}
