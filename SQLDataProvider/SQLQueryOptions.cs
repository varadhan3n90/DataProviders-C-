// <copyright file="SQLQueryOptions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DataAccessService.SQL
{
    using System.Collections.Generic;
    using System.Data;

    /// <summary>
    /// Sql query options.
    /// </summary>
    public class SQLQueryOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SQLQueryOptions"/> class.
        /// </summary>
        public SQLQueryOptions()
        {
            CommandProperties = new Dictionary<string, object>();
        }

        /// <summary>
        /// Gets or sets the Input command text.
        /// </summary>
        public string CommandText { get; set; }

        /// <summary>
        /// Gets the command properties.
        /// </summary>
        public IDictionary<string, object> CommandProperties { get; private set; }

        /// <summary>
        /// Gets or sets the reader type.
        /// </summary>
        public SQLReaderType ReaderType { get; set; }

        /// <summary>
        /// Gets or sets the command type.
        /// </summary>
        public CommandType CmdType { get; set; } = CommandType.Text;
    }
}
