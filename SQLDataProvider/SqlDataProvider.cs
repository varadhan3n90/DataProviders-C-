// <copyright file="SqlDataProvider.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DataAccessService.SQL
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Sql data provider.
    /// </summary>
    public sealed class SQLDataProvider : IDataProvider, IDisposable
    {
        private readonly string _message = "Connection string cannot be empty";
        private readonly SQLDataProviderOptions _providerOptions;
        private SqlConnection _connection;
        private SqlCommand _sqlCommand;

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLDataProvider"/> class.
        /// </summary>
        /// <param name="options">Options to create sql provider.</param>
        public SQLDataProvider(IOptions<SQLDataProviderOptions> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _providerOptions = options.Value;
        }

        /// <summary>
        /// Gets the Provider name.
        /// </summary>
        public string DataProviderName { get; } = DataProviders.Sql;

        /// <summary>
        /// Delete data not explicitly required. Use Get data with delete statement.
        /// </summary>
        /// <param name="options">Options.</param>
        /// <returns>NotImplementedException.</returns>
        public Task DeleteData(object options)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Used to get data from SQL server.
        /// </summary>
        /// <typeparam name="T">The class object expected as output.</typeparam>
        /// <param name="queryOptions"><see cref="SQLQueryOptions"/>.</param>
        /// <param name="dataPropertyMapper">Call back function to map raw object to required output type.</param>
        /// <returns>Data mapped by data property mapper.</returns>
        public async Task<T> GetData<T>(object queryOptions, Action<T, object> dataPropertyMapper)
            where T : new()
        {
            if (string.IsNullOrEmpty(_providerOptions.ConnectionString))
            {
                throw new Exception(_message);
            }

            if (!(queryOptions is SQLQueryOptions q))
            {
                throw new ArgumentNullException(nameof(queryOptions));
            }

            var t = Activator.CreateInstance<T>();
            var dbobject = await GetDbReader(q).ConfigureAwait(false);
            dataPropertyMapper?.Invoke(t, dbobject);
            Dispose();
            return t;
        }

        /// <summary>
        /// Used to store data.
        /// </summary>
        /// <typeparam name="T">The class object expected as output.</typeparam>
        /// <param name="options"><see cref="SQLQueryOptions"/>.</param>
        /// <param name="t">Data to be stored.</param>
        /// <returns>Data stored.</returns>
        public async Task<T> StoreData<T>(object options, T t)
        {
            if (!(options is SQLQueryOptions opt))
            {
                throw new ArgumentNullException(nameof(options));
            }

            opt.ReaderType = SQLReaderType.NonQuery;
            await GetDbReader(opt).ConfigureAwait(false);
            Dispose();
            return t;
        }

        /// <summary>
        /// Used to update data. This method is not implemented.
        /// GetData can be directly used to update data as well, since this is sql data provider.
        /// </summary>
        /// <typeparam name="T">The type of object to be updated.</typeparam>
        /// <param name="options">This is not used.</param>
        /// <param name="t">object to be updated.</param>
        /// <returns>Throws not implemented exception.</returns>
        public Task<T> UpdateData<T>(object options, T t)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// To dispose this object.
        /// </summary>
        public void Dispose()
        {
            if (_connection.State != ConnectionState.Closed)
            {
                _connection.Close();
            }

            _sqlCommand?.Dispose();
            _connection?.Dispose();
        }

        private async Task<object> GetDbReader(SQLQueryOptions queryOptions)
        {
            _connection = new SqlConnection(_providerOptions.ConnectionString);
            _connection.Open();
            _sqlCommand = new SqlCommand(queryOptions.CommandText, _connection)
            {
                CommandTimeout = _providerOptions.Timeout,
                CommandType = queryOptions.CmdType,
            };

            if (queryOptions.CommandProperties != null)
            {
                foreach (var item in queryOptions.CommandProperties)
                {
                    _sqlCommand.Parameters.AddWithValue(item.Key, item.Value);
                }
            }

            switch (queryOptions.ReaderType)
            {
                case SQLReaderType.Scalar:
                    return await _sqlCommand.ExecuteScalarAsync().ConfigureAwait(false);

                case SQLReaderType.NonQuery:
                    return await _sqlCommand.ExecuteNonQueryAsync().ConfigureAwait(false);

                case SQLReaderType.XML:
                    return await _sqlCommand.ExecuteXmlReaderAsync().ConfigureAwait(false);

                default:
                    return await _sqlCommand.ExecuteReaderAsync().ConfigureAwait(false);
            }
        }
    }
}
