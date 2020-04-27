// <copyright file="BlobDataProviderOptions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DataAccessService.Blob
{
    /// <summary>
    /// ConnectionString and provider name.
    /// </summary>
    public class BlobDataProviderOptions
    {
        /// <summary>
        /// Gets or sets connection string to storage account.
        /// </summary>
        public string BlobConnectionString { get; set; }

        /// <summary>
        /// Gets or sets custom provider name.
        /// </summary>
        public string BlobProviderName { get; set; }
    }
}
