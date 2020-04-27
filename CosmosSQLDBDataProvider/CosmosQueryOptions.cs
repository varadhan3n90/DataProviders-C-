// <copyright file="CosmosQueryOptions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DataAccessService.CosmosSql
{
    /// <summary>
    /// Query options for using cosmos db.
    /// </summary>
    public class CosmosQueryOptions
    {
        /// <summary>
        /// Gets or sets database name.
        /// </summary>
        public string DataBaseName { get; set; }

        /// <summary>
        /// Gets or sets collection name.
        /// </summary>
        public string CollectionName { get; set; }

        /// <summary>
        /// Gets or sets document id.
        /// </summary>
        public string DocumentId { get; set; }

        /// <summary>
        /// Gets or sets query.
        /// </summary>
        public string Query { get; set; }
    }
}
