// <copyright file="ServiceBusDataProvider.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DataAccessService.ServiceBusProvider
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.ServiceBus.Core;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;

    /// <summary>
    /// Service bus data provider (Topic - subscription).
    /// </summary>
    public class ServiceBusDataProvider : IDataProvider
    {
        private readonly TopicClient _topicClient;

        private readonly IOptions<ServiceBusConfiguration> _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusDataProvider"/> class.
        /// </summary>
        /// <param name="options"><see cref="ServiceBusConfiguration"/> configuration options.</param>
        public ServiceBusDataProvider(IOptions<ServiceBusConfiguration> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _topicClient = new TopicClient(options.Value.ConnectionString, options.Value.TopicName);
            _options = options;
        }

        /// <summary>
        /// Gets Provider name.
        /// </summary>
        public string DataProviderName
        {
            get
            {
                return DataProviders.ServiceBusProvider;
            }
        }

        /// <summary>
        /// Get data from service bus.
        /// </summary>
        /// <typeparam name="T">The class object expected as output.</typeparam>
        /// <param name="queryOptions"><see cref="ServiceBusDLQConfiguration"/>.</param>
        /// <param name="dataPropertyMapper">Call back function to map raw object to required output type.</param>
        /// <returns>Data mapped by data property mapper.</returns>
        public async Task<T> GetData<T>(object queryOptions, Action<T, object> dataPropertyMapper)
            where T : new()
        {
            var t = Activator.CreateInstance<T>();
            switch (queryOptions)
            {
                case ServiceBusDLQConfiguration serviceBusDLQConfiguration:

                    var deadletterReceiver = new MessageReceiver(_options.Value.ConnectionString, EntityNameHelper.FormatDeadLetterPath($"{serviceBusDLQConfiguration.TopicName}/Subscriptions/{serviceBusDLQConfiguration.SubscriptionName}"), serviceBusDLQConfiguration.ReceiveMode, RetryPolicy.Default, int.MaxValue);

                    IList<Message> messages = new List<Message>();
                    while (true)
                    {
                        IList<Message> msgs;

                        // receive a message
                        if (serviceBusDLQConfiguration.ReceiveMode == ReceiveMode.PeekLock)
                        {
                            msgs = await deadletterReceiver.PeekAsync(serviceBusDLQConfiguration.MessageCount == 0 ? serviceBusDLQConfiguration.MessageBatchSize : serviceBusDLQConfiguration.MessageCount).ConfigureAwait(false);
                        }
                        else
                        {
                            msgs = await deadletterReceiver.ReceiveAsync(serviceBusDLQConfiguration.MessageCount == 0 ? serviceBusDLQConfiguration.MessageBatchSize : serviceBusDLQConfiguration.MessageCount).ConfigureAwait(false);
                        }

                        if (msgs != null && msgs.Count > 0)
                        {
                            messages = messages.Union(msgs).ToList();
                            if (serviceBusDLQConfiguration.MessageCount != 0 && messages.Count >= serviceBusDLQConfiguration.MessageCount)
                            {
                                break;
                            }
                        }
                        else
                        {
                            // DLQ was empty on last receive attempt
                            break;
                        }
                    }

                    t = (T)messages;
                    await deadletterReceiver.CloseAsync().ConfigureAwait(false);
                    return t;
            }

            throw new ArgumentNullException(nameof(queryOptions));
        }

        /// <summary>
        /// Used to send messsage to service bus.
        /// </summary>
        /// <typeparam name="T">Type of object to be stored.</typeparam>
        /// <param name="options"><see cref="ServiceBusMessageOptions"/> message options.</param>
        /// <param name="t">Object to be stored.</param>
        /// <returns>The stored object.</returns>
        public async Task<T> StoreData<T>(object options, T t)
        {
            var serializer = new DataContractSerializer(typeof(string));
            using (var ms = new MemoryStream())
            {
                using (var binaryDictionaryWriter = XmlDictionaryWriter.CreateBinaryWriter(ms))
                {
                    serializer.WriteObject(binaryDictionaryWriter, JsonConvert.SerializeObject(t));
                    binaryDictionaryWriter.Flush();
                }

                var message = new Message(ms.ToArray());
                if (options != null && options is ServiceBusMessageOptions sbm)
                {
                    sbm.MessageHandler(message);
                }

                await _topicClient.SendAsync(message).ConfigureAwait(false);
            }

            return t;
        }

        /// <summary>
        /// Update data. This is not implemented.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="options">options.</param>
        /// <param name="t">object.</param>
        /// <returns>Task.</returns>
        public Task<T> UpdateData<T>(object options, T t)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Used to delete data from service bus.
        /// </summary>
        /// <param name="options"><see cref="ServiceBusDeleteFromDLQConfiguration"/> options to delete data.</param>
        /// <returns>Task.</returns>
        public async Task DeleteData(object options)
        {
            switch (options)
            {
                case ServiceBusDeleteFromDLQConfiguration serviceBusDeleteFromDLQConfiguration:
                    if (serviceBusDeleteFromDLQConfiguration.MessageSequenceNumbers != null && serviceBusDeleteFromDLQConfiguration.MessageSequenceNumbers.Any())
                    {
                        var messageSender = new MessageSender(_options.Value.ConnectionString, serviceBusDeleteFromDLQConfiguration.TopicName, RetryExponential.Default);
                        var deadletterReceiver = new MessageReceiver(_options.Value.ConnectionString, EntityNameHelper.FormatDeadLetterPath($"{serviceBusDeleteFromDLQConfiguration.TopicName}/Subscriptions/{serviceBusDeleteFromDLQConfiguration.SubscriptionName}"), ReceiveMode.PeekLock, RetryPolicy.Default, int.MaxValue);

                        IList<Message> messages = new List<Message>();
                        while (true)
                        {
                            IList<Message> msgs;
                            msgs = await deadletterReceiver.ReceiveAsync(serviceBusDeleteFromDLQConfiguration.MaxMessageCount, serviceBusDeleteFromDLQConfiguration.OperationTimeout).ConfigureAwait(false);
                            if (msgs != null && msgs.Count > 0)
                            {
                                messages = messages.Union(msgs).ToList();
                            }
                            else
                            {
                                // DLQ was empty on last receive attempt
                                break;
                            }
                        }

                        if (messages != null)
                        {
                            foreach (var message in messages)
                            {
                                if (serviceBusDeleteFromDLQConfiguration.MessageSequenceNumbers.Contains(message.SystemProperties.SequenceNumber))
                                {
                                    var body = Encoding.UTF8.GetString(message.Body).Replace(",", ", ").Replace("\":\"", "\": \"");
                                    serviceBusDeleteFromDLQConfiguration.ReflaggedMessageTexts.Add(body.Substring(body.IndexOf('{'), body.LastIndexOf('}') - body.IndexOf('{') + 1), true);
                                    var messageToSend = message.Clone();
                                    if (serviceBusDeleteFromDLQConfiguration.DeleteFromDLQ)
                                    {
                                        await deadletterReceiver.CompleteAsync(message.SystemProperties.LockToken).ConfigureAwait(false);
                                    }

                                    message.UserProperties["DeadLetterReason"] = null;
                                    message.UserProperties["DeadLetterErrorDescription"] = null;
                                    await messageSender.SendAsync(messageToSend).ConfigureAwait(false);
                                }
                            }

                            await messageSender.CloseAsync().ConfigureAwait(false);
                            await deadletterReceiver.CloseAsync().ConfigureAwait(false);
                        }
                    }

                    break;
            }
        }
    }
}