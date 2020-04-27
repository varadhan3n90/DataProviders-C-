// <copyright file="SQLDataProviderOptions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DataAccessService.SQL
{
    /// <summary>
    /// SQLDataProviderOptions.
    /// </summary>
    public class SQLDataProviderOptions
    {
        /// <summary>
        /// Gets or sets the timeout.
        /// </summary>
        public int Timeout { get; set; } = 30;

        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        public string ConnectionString { get; set; }
    }
}
