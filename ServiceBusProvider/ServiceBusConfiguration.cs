// <copyright file="ServiceBusConfiguration.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DataAccessService.ServiceBusProvider
{
    /// <summary>
    /// Service bus configuration.
    /// </summary>
    public class ServiceBusConfiguration
    {
        /// <summary>
        /// Gets or sets the service bus connection string.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets topic name for publish subscribe.
        /// </summary>
        public string TopicName { get; set; }
    }
}