// <copyright file="RedisCacheProvider.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DataAccessService.RedisCache
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Caching.Distributed;
    using Newtonsoft.Json;

    /// <summary>
    /// Redis cache provider.
    /// </summary>
    public class RedisCacheProvider : IDataProvider
    {
        private readonly IDistributedCache _distributedCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisCacheProvider"/> class.
        /// </summary>
        /// <param name="cache">Cache options.</param>
        public RedisCacheProvider(IDistributedCache cache)
        {
            _distributedCache = cache;
        }

        /// <summary>
        /// Gets data provider name.
        /// </summary>
        public string DataProviderName { get; } = DataProviders.RedisCache;

        /// <summary>
        /// Delete object from cache. Not implemented.
        /// </summary>
        /// <param name="options">Options to delete key form cache.</param>
        /// <returns>Task.</returns>
        public Task DeleteData(object options)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Used to get data from redis cache.
        /// </summary>
        /// <typeparam name="T">The class object expected as output.</typeparam>
        /// <param name="queryOptions"><see cref="RedisCacheQueryOptions"/>.</param>
        /// <param name="dataPropertyMapper">Call back function to map raw object to required output type.</param>
        /// <returns>Data mapped by data property mapper.</returns>
        public async Task<T> GetData<T>(object queryOptions, Action<T, object> dataPropertyMapper)
            where T : new()
        {
            if (!(queryOptions is RedisCacheQueryOptions options))
            {
                throw new ArgumentNullException(nameof(queryOptions));
            }

            string value;
            try
            {
                value = await _distributedCache.GetStringAsync(options.Key).ConfigureAwait(false);
            }
            catch
            {
                return default;
            }

            if (value == null)
            {
                return default;
            }

            var inst = JsonConvert.DeserializeObject<T>(value);
            dataPropertyMapper?.Invoke(inst, value);
            return inst;
        }

        /// <summary>
        /// Function to store in redis cache.
        /// </summary>
        /// <typeparam name="T">The class data that will be stored.</typeparam>
        /// <param name="options"><see cref="CosmosQueryOptions"/> Options required to store blob.</param>
        /// <param name="t">Object that will be serialized and stored.</param>
        /// <returns>The object T that is stored.</returns>
        public async Task<T> StoreData<T>(object options, T t)
        {
            if (!(options is RedisCacheQueryOptions queryoptions))
            {
                throw new ArgumentNullException(nameof(options));
            }

            var o = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = new TimeSpan(6, 0, 0),
            };

            if (t is string)
            {
                await _distributedCache.SetStringAsync(queryoptions.Key, t.ToString(), o).ConfigureAwait(false);
            }
            else
            {
                await _distributedCache.SetStringAsync(queryoptions.Key, JsonConvert.SerializeObject(t), o)
                    .ConfigureAwait(false);
            }

            return t;
        }

        /// <summary>
        /// Used to update data.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="options">Options to update.</param>
        /// <param name="t">Object to update.</param>
        /// <returns>Task.</returns>
        public Task<T> UpdateData<T>(object options, T t)
        {
            throw new NotImplementedException();
        }
    }
}
