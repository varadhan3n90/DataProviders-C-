// <copyright file="CosmosSqlProviderOptions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DataAccessService.CosmosSql
{
    using System;

    /// <summary>
    /// CosmosSqlProviderOptions.
    /// </summary>
    public class CosmosSqlProviderOptions
    {
        /// <summary>
        /// Gets or sets the endpoint Uri.
        /// </summary>
        public Uri EndpointUri { get; set; }

        /// <summary>
        /// Gets or sets the Key.
        /// </summary>
        public string Key { get; set; }
    }
}
