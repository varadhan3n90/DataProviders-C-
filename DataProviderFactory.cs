// <copyright file="DataProviderFactory.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DataAccessService
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Data provider factory to initialize data provider.
    /// </summary>
    public class DataProviderFactory
    {
        private readonly List<IDataProvider> _dataProviders;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataProviderFactory"/> class.
        /// </summary>
        /// <param name="dataProviders">List of providers.</param>
        public DataProviderFactory(IEnumerable<IDataProvider> dataProviders)
        {
            _dataProviders = dataProviders.ToList();
        }

        /// <summary>
        /// Get a data provider.
        /// </summary>
        /// <param name="dataProviderName">DataProviderName.</param>
        /// <returns>DataProvider.</returns>
        public IDataProvider GetDataProvider(string dataProviderName)
        {
            return _dataProviders.Find(x => x.DataProviderName.Equals(dataProviderName, System.StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
