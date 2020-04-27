// <copyright file="ServiceBusDeleteFromDLQConfiguration.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DataAccessService.ServiceBusProvider
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// ServiceBusDeleteFromDLQConfiguration.
    /// </summary>
    public class ServiceBusDeleteFromDLQConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusDeleteFromDLQConfiguration"/> class.
        /// </summary>
        public ServiceBusDeleteFromDLQConfiguration()
        {
            ReflaggedMessageTexts = new Dictionary<string, bool>();
            MessageSequenceNumbers = new List<long>();
        }

        /// <summary>
        /// Gets or sets the topic name.
        /// </summary>
        public string TopicName { get; set; }

        /// <summary>
        /// Gets or sets the subscription name.
        /// </summary>
        public string SubscriptionName { get; set; }

        /// <summary>
        /// Gets or sets the operation timeout.
        /// </summary>
        public TimeSpan OperationTimeout { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether should delete from dlq.
        /// </summary>
        public bool DeleteFromDLQ { get; set; }

        /// <summary>
        /// Gets message sequence numbers.
        /// </summary>
        public List<long> MessageSequenceNumbers { get; private set; }

        /// <summary>
        /// Gets or sets max messsage count.
        /// </summary>
        public int MaxMessageCount { get; set; }

        /// <summary>
        /// Gets dictonary of ReflaggedMessageTexts.
        /// </summary>
        public Dictionary<string, bool> ReflaggedMessageTexts { get; private set; }
    }
}