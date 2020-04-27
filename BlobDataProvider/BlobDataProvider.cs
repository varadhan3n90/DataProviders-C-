// <copyright file="BlobDataProvider.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DataAccessService.Blob
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Serialization;
    using Microsoft.Azure.Storage;
    using Microsoft.Azure.Storage.Blob;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;

    /// <summary>
    /// BlobDataProvider will be used to upload and download files in xml or json format to blob.
    /// </summary>
    public class BlobDataProvider : IDataProvider
    {
        private readonly BlobDataProviderOptions _options;
        private readonly string _optionsNotPassed = "BlobQueryOptions not passed";
        private readonly string _invalidConnectionString = "Invalid blob connection string";
        private CloudBlobClient _blobClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobDataProvider"/> class.
        /// </summary>
        /// <param name="dataProviderOptions">Blob Data provider options.</param>
        public BlobDataProvider(IOptions<BlobDataProviderOptions> dataProviderOptions)
        {
            if (dataProviderOptions == null)
            {
                throw new ArgumentNullException(nameof(dataProviderOptions));
            }

            _options = dataProviderOptions.Value;
            InitializeBlob();
        }

        /// <summary>
        /// Gets provider name.
        /// </summary>
        public string DataProviderName
        {
            get
            {
                if (string.IsNullOrEmpty(_options.BlobProviderName))
                {
                    return DataProviders.BlobStorage;
                }
                else
                {
                    return _options.BlobProviderName;
                }
            }
        }

        /// <summary>
        /// Used to get data from blob storage.
        /// </summary>
        /// <typeparam name="T">The class object expected as output.</typeparam>
        /// <param name="queryOptions"><see cref="BlobQueryOptions"/>.</param>
        /// <param name="dataPropertyMapper">Call back function to map raw object to required output type.</param>
        /// <returns>Data mapped by data property mapper.</returns>
        public async Task<T> GetData<T>(object queryOptions, Action<T, object> dataPropertyMapper)
            where T : new()
        {
            if (!(queryOptions is BlobQueryOptions query))
            {
                throw new Exception(_optionsNotPassed);
            }

            var t = Activator.CreateInstance<T>();
            var blobContainer = _blobClient.GetContainerReference(query.ContainerName);
            var blob = blobContainer.GetBlobReference(query.BlobName);
            if (!blob.ExistsAsync().Result)
            {
                return default;
            }

            string blobObject;
            using (var memoryStream = new MemoryStream())
            {
                await blob.DownloadToStreamAsync(memoryStream).ConfigureAwait(false);
                blobObject = Encoding.UTF8.GetString(memoryStream.ToArray());
            }

            dataPropertyMapper?.Invoke(t, blobObject);
            return t;
        }

        /// <summary>
        /// Function to store data in blob.
        /// </summary>
        /// <typeparam name="T">The class data that will be stored.</typeparam>
        /// <param name="options"><see cref="BlobQueryOptions"/> Options required to store blob.</param>
        /// <param name="t">Object that will be serialized and stored.</param>
        /// <returns>The object T that is stored.</returns>
        public async Task<T> StoreData<T>(object options, T t)
        {
            if (!(options is BlobQueryOptions query))
            {
                throw new Exception(_optionsNotPassed);
            }

            var blobContainer = _blobClient.GetContainerReference(query.ContainerName);
            var blob = blobContainer.GetBlockBlobReference(query.BlobName);
            switch (query.Serialization)
            {
                case BlobSerializationType.Json:
                    var serializedValue = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(t));
                    using (var stream = new MemoryStream(serializedValue))
                    {
                        blob.Properties.ContentType = "text/json";
                        await blob.UploadFromStreamAsync(stream).ConfigureAwait(false);
                    }

                    break;
                case BlobSerializationType.Xml:
                    using (var textWriter = new StringWriter())
                    {
                        var xmlSerializer = new XmlSerializer(t.GetType());
                        xmlSerializer.Serialize(textWriter, t);
                        blob.Properties.ContentType = "text/xml";
                        await blob.UploadTextAsync(textWriter.ToString()).ConfigureAwait(false);
                    }

                    break;
            }

            return t;
        }

        /// <summary>
        /// Please use StoreData to rewrite copy of file in blob.
        /// This funtion is not implemented.
        /// </summary>
        /// <typeparam name="T">Not implemented.</typeparam>
        /// <param name="options">.</param>
        /// <param name="t">Do not use.</param>
        /// <returns>Exception.</returns>
        public Task<T> UpdateData<T>(object options, T t)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Used to delete a given blob.
        /// </summary>
        /// <param name="options"><see cref="BlobQueryOptions"/>Options required to delete blob.</param>
        /// <returns>Task to run delete async.</returns>
        public async Task DeleteData(object options)
        {
            if (!(options is BlobQueryOptions query))
            {
                throw new Exception(_optionsNotPassed);
            }

            var blobContainer = _blobClient.GetContainerReference(query.ContainerName);
            var blob = blobContainer.GetBlobReference(query.BlobName);
            await blob.DeleteIfExistsAsync().ConfigureAwait(false);
        }

        private void InitializeBlob()
        {
            if (string.IsNullOrEmpty(_options.BlobConnectionString))
            {
                throw new Exception(_invalidConnectionString);
            }

            var storageAccount = CloudStorageAccount.Parse(_options.BlobConnectionString);
            _blobClient = storageAccount.CreateCloudBlobClient();
        }
    }
}
