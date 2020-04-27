// <copyright file="TableOptions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DataAccessService.TableStorageDataProvider
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Table options.
    /// </summary>
    public class TableOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TableOptions"/> class.
        /// </summary>
        public TableOptions()
        {
            FieldsAllowedForTimeZoneConversion = new List<string>();
        }

        /// <summary>
        /// Gets or sets table name.
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Gets or sets row key.
        /// </summary>
        public string RowKey { get; set; }

        /// <summary>
        /// Gets or sets row key generator.
        /// </summary>
        public Func<object, string> RowKeyGenerator { get; set; }

        /// <summary>
        /// Gets or sets partition key.
        /// </summary>
        public string PartitionKey { get; set; }

        /// <summary>
        /// Gets FieldsAllowedForTimeZoneConversion.
        /// </summary>
        public List<string> FieldsAllowedForTimeZoneConversion { get; private set; }
    }
}
