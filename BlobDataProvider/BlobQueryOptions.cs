// <copyright file="BlobQueryOptions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DataAccessService.Blob
{
    /// <summary>
    /// Query options used to get blob data.
    /// </summary>
    public class BlobQueryOptions
    {
        /// <summary>
        /// Gets or sets blob container name.
        /// </summary>
        public string ContainerName { get; set; }

        /// <summary>
        /// Gets or sets Blob name.
        /// </summary>
        public string BlobName { get; set; }

        /// <summary>
        /// Gets or sets serialization type.
        /// </summary>
        public BlobSerializationType Serialization { get; set; }
    }
}
