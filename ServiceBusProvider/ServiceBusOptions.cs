// <copyright file="ServiceBusOptions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DataAccessService.ServiceBusProvider
{
    using System;

    /// <summary>
    /// ServiceBusOptions.
    /// </summary>
    public class ServiceBusOptions
    {
        /// <summary>
        /// Gets or sets subscription name.
        /// </summary>
        public string SubscriptionName { get; set; }

        /// <summary>
        /// Gets or sets timeout.
        /// </summary>
        public TimeSpan Timeout { get; set; }
    }
}