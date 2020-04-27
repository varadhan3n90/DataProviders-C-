// <copyright file="ServiceBusMessageOptions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DataAccessService.ServiceBusProvider
{
    using System;
    using Microsoft.Azure.ServiceBus;

    /// <summary>
    /// ServiceBusMessageOptions.
    /// </summary>
    public class ServiceBusMessageOptions
    {
        /// <summary>
        /// Gets or sets action on message.
        /// </summary>
        public Action<Message> MessageHandler { get; set; }
    }
}