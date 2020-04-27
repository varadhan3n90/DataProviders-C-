// <copyright file="IDataProvider.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DataAccessService
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides CRUD operations.
    /// </summary>
    public interface IDataProvider
    {
        /// <summary>
        /// Gets the data provider name.
        /// </summary>
        string DataProviderName { get; }

        /// <summary>
        /// Used to get data.
        /// </summary>
        /// <typeparam name="T">Type of data to get.</typeparam>
        /// <param name="queryOptions">The options to query data.</param>
        /// <param name="dataPropertyMapper">Mapper to map retrieved data to return object.</param>
        /// <returns>Object of type T mapped by mapper.</returns>
        Task<T> GetData<T>(object queryOptions, Action<T, object> dataPropertyMapper)
            where T : new();

        /// <summary>
        /// Used to create/ store data.
        /// </summary>
        /// <typeparam name="T">Type of data to be stored.</typeparam>
        /// <param name="options">Options required to store data.</param>
        /// <param name="t">The object to be stored.</param>
        /// <returns>Object that is stored.</returns>
        Task<T> StoreData<T>(object options, T t);

        /// <summary>
        /// Used to update data.
        /// </summary>
        /// <typeparam name="T">Type of data to update.</typeparam>
        /// <param name="options">Options.</param>
        /// <param name="t">Object to update.</param>
        /// <returns>Updated object.</returns>
        Task<T> UpdateData<T>(object options, T t);

        /// <summary>
        /// Used to delete data.
        /// </summary>
        /// <param name="options">Options to delete.</param>
        /// <returns>void.</returns>
        Task DeleteData(object options);
    }

    /// <summary>
    /// Default Data Provider Names.
    /// </summary>
    public static class DataProviders
    {
        /// <summary>
        /// Default AzureTableStorage provider name.
        /// </summary>
        public const string AzureTableStorage = "AzureTableStorage";

        /// <summary>
        /// Default BlobStorage provider name.
        /// </summary>
        public const string BlobStorage = "BlobStorage";

        /// <summary>
        /// Default RedisCache provider name.
        /// </summary>
        public const string RedisCache = "RedisCache";

        /// <summary>
        /// Default CosmosSql provider name.
        /// </summary>
        public const string CosmosSql = "CosmosSql";

        /// <summary>
        /// Default Sql provider name.
        /// </summary>
        public const string Sql = "Sql";

        /// <summary>
        /// Default ServiceBusProvider provider name.
        /// </summary>
        public const string ServiceBusProvider = "ServiceBusProvider";
    }
}
