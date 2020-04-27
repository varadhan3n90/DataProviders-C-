// <copyright file="TableStorageOptions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DataAccessService.TableStorageDataProvider
{
    /// <summary>
    /// https://blog.completecoder.net/2014/01/.
    /// </summary>
    public class TableStorageOptions
    {
        /// <summary>
        /// Gets or sets Storage connection string.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string StorageConnectionString { get; set; }

        /// <summary>
        /// Gets or sets Tables to create at startup.
        /// </summary>
        public string TablesToCreateAtStartup { get; set; }

        /// <summary>
        /// Gets or sets Source timezone.
        /// </summary>
        public string SourceTimeZone { get; set; }
    }
}
