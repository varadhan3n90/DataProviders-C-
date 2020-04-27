// <copyright file="TableStorageQueryOptions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DataAccessService.TableStorageDataProvider
{
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Table Storage query options.
    /// </summary>
    /// <typeparam name="T">Table Entity Object.</typeparam>
    public class TableStorageQueryOptions<T>
        where T : new()
    {
        /// <summary>
        /// Gets or sets entity resolver.
        /// </summary>
        public EntityResolver<T> Resolver { get; set; }

        /// <summary>
        /// Gets or sets projection query.
        /// </summary>
        public TableQuery<DynamicTableEntity> ProjectionQuery { get; set; }

        /// <summary>
        /// Gets or sets the table name.
        /// </summary>
        public string TableName { get; set; }
    }
}
