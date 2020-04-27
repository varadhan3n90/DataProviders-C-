// <copyright file="ServiceBusDLQConfiguration.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DataAccessService.ServiceBusProvider
{
    using System;
    using Microsoft.Azure.ServiceBus;

    /// <summary>
    /// ServiceBusDLQConfiguration.
    /// </summary>
    public class ServiceBusDLQConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusDLQConfiguration"/> class.
        /// </summary>
        public ServiceBusDLQConfiguration()
        {
            this.ReceiveMode = ReceiveMode.PeekLock;
        }

        /// <summary>
        /// Gets or sets topic name.
        /// </summary>
        public string TopicName { get; set; }

        /// <summary>
        /// Gets or sets subscription name.
        /// </summary>
        public string SubscriptionName { get; set; }

        /// <summary>
        /// Gets or sets operation timeout.
        /// </summary>
        public TimeSpan OperationTimeout { get; set; }

        /// <summary>
        /// Gets or sets message count.
        /// </summary>
        public int MessageCount { get; set; }

        /// <summary>
        /// Gets or sets receive mode.
        /// </summary>
        public ReceiveMode ReceiveMode { get; set; }

        /// <summary>
        /// Gets or sets message batch size.
        /// </summary>
        public int MessageBatchSize { get; set; }
    }
}