// <copyright file="SQLReaderType.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DataAccessService.SQL
{
    /// <summary>
    /// SQLReaderType.
    /// </summary>
    public enum SQLReaderType
    {
        /// <summary>
        /// query.
        /// </summary>
        Query,

        /// <summary>
        /// scalar.
        /// </summary>
        Scalar,

        /// <summary>
        /// xml.
        /// </summary>
        XML,

        /// <summary>
        /// non-query.
        /// </summary>
        NonQuery,
    }
}
