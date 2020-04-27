// <copyright file="EntityConverter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DataAccessService.TableStorageDataProvider
{
    using System;
    using System.Collections.Generic;
    using Microsoft.WindowsAzure.Storage.Table;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Class to convert object to dynamic entity.
    /// </summary>
    public static class EntityConverter
    {
        /// <summary>
        ///  Convert any JSON.NET seriliazable Object into a Dynamic Table Entity.
        /// </summary>
        /// <typeparam name="TValue">Object Type to be Seriliazed.</typeparam>
        /// <param name="poco">.NET Object to made into Table Object.</param>
        /// <param name="partitionKeySelector">Function to Select Partition Key Object.</param>
        /// <param name="rowKeySelector">Function to Select Row key from Object.</param>
        /// <param name="timeStamp">Timestamp on Table Entity.</param>
        /// <param name="etag">Etag on Table Entity.</param>
        /// <param name="jsonSerializer">Optional Custom Json Serializer.</param>
        /// <param name="convertToTimeZone">convert to timezone.</param>
        /// <param name="fieldsAllowedForTimeZoneConversion">allowed.</param>
        /// <returns>Dynamic Table Entity to be stored in Azure Table Storage.</returns>
        public static DynamicTableEntity ConvertToDynamicTableEntity<TValue>(
            TValue poco,
            Func<TValue, string> partitionKeySelector,
            Func<TValue, string> rowKeySelector,
            DateTimeOffset? timeStamp = null,
            string etag = null,
            JsonSerializer jsonSerializer = null,
            string convertToTimeZone = null,
            List<string> fieldsAllowedForTimeZoneConversion = null)
        {
            return ConvertToDynamicTableEntity(
                poco,
                partitionKeySelector?.Invoke(poco),
                rowKeySelector(poco),
                timeStamp,
                etag,
                jsonSerializer,
                convertToTimeZone,
                fieldsAllowedForTimeZoneConversion);
        }

        /// <summary>
        ///  Convert any JSON.NET seriliazable Object into a Dynamic Table Entity.
        /// </summary>
        /// <param name="poco">.NET Object to made into Table Object.</param>
        /// <param name="partitionKey">Function to Select Partition Key Object.</param>
        /// <param name="rowKey">Function to Select Row key from Object.</param>
        /// <param name="timeStamp">Timestamp on Table Entity.</param>
        /// <param name="etag">Etag on Table Entity.</param>
        /// <param name="jsonSerializer">Optional Custom Json Serializer.</param>
        /// <param name="convertToTimeZone">Timezone to convert to.</param>
        /// <param name="fieldsAllowedForTimeZoneConversion">fields for conversion.</param>
        /// <returns>Dynamic Table Entity to be stored in Azure Table Storage.</returns>
        public static DynamicTableEntity ConvertToDynamicTableEntity(
            object poco,
            string partitionKey = null,
            string rowKey = null,
            DateTimeOffset? timeStamp = null,
            string etag = null,
            JsonSerializer jsonSerializer = null,
            string convertToTimeZone = null,
            List<string> fieldsAllowedForTimeZoneConversion = null)
        {
            var dynamicTableEntity = new DynamicTableEntity
            {
                RowKey = rowKey,
                PartitionKey = partitionKey,
                Properties = new Dictionary<string, EntityProperty>(),
            };

            if (timeStamp.HasValue)
            {
                dynamicTableEntity.Timestamp = timeStamp.Value;
            }

            if (!string.IsNullOrWhiteSpace(etag))
            {
                dynamicTableEntity.ETag = etag;
            }

            var jObject = jsonSerializer != null ? JObject.FromObject(poco, jsonSerializer) : JObject.FromObject(poco);

            foreach (var item in jObject.Values<JProperty>())
            {
                KeyValuePair<string, EntityProperty>? pair = WriteToEntityProperty(item, convertToTimeZone, fieldsAllowedForTimeZoneConversion);
                if (pair.HasValue)
                {
                    dynamicTableEntity.Properties.Add(pair.Value);
                }
            }

            return dynamicTableEntity;
        }

        /// <summary>
        ///  Converts a dynamic table entity to .NET Object.
        /// </summary>
        /// <typeparam name="TOutput">Desired Object Type.</typeparam>
        /// <param name="entity">Dynamic table Entity.</param>
        /// <param name="convertToTimeZone">Converts any datetime field in table entity to the required time zone.</param>
        /// <param name="fieldsAllowedForTimeZoneConversion">fields allowed for time zone conversion.</param>
        /// <returns>Output Object.</returns>
        public static TOutput ConvertTo<TOutput>(DynamicTableEntity entity, string convertToTimeZone = null, List<string> fieldsAllowedForTimeZoneConversion = null)
        {
            if (string.IsNullOrEmpty(convertToTimeZone))
            {
                return ConvertTo<TOutput>(entity?.Properties, TimeZoneInfo.Utc.ToString());
            }

            return ConvertTo<TOutput>(entity?.Properties, convertToTimeZone, fieldsAllowedForTimeZoneConversion);
        }

        /// <summary>
        /// Entity to property.
        /// </summary>
        /// <param name="property">property.</param>
        /// <param name="convertToTimeZone">convert to timezone.</param>
        /// <param name="fieldsAllowedForTimeZoneConversion">fields for timezone conversion.</param>
        /// <returns>key value pair.</returns>
        public static KeyValuePair<string, EntityProperty>? WriteToEntityProperty(JProperty property, string convertToTimeZone = null, List<string> fieldsAllowedForTimeZoneConversion = null)
        {
            var value = property?.Value;
            var name = property.Name;
            switch (value.Type)
            {
                case JTokenType.Bytes:
                    return new KeyValuePair<string, EntityProperty>(name, new EntityProperty(value.ToObject<byte[]>()));
                case JTokenType.Boolean:
                    return new KeyValuePair<string, EntityProperty>(name, new EntityProperty(value.ToObject<bool>()));
                case JTokenType.Date:
                    if (!string.IsNullOrEmpty(convertToTimeZone) && (fieldsAllowedForTimeZoneConversion == null || (fieldsAllowedForTimeZoneConversion != null && fieldsAllowedForTimeZoneConversion.Contains(name))))
                    {
                        var d = new DateTime(value.ToObject<DateTime>().Ticks, DateTimeKind.Unspecified);
                        return new KeyValuePair<string, EntityProperty>(name, new EntityProperty(TimeZoneInfo.ConvertTimeToUtc(d, TimeZoneInfo.FindSystemTimeZoneById(convertToTimeZone))));
                    }
                    else
                    {
                        return new KeyValuePair<string, EntityProperty>(name, new EntityProperty(value.ToObject<DateTime>()));
                    }

                case JTokenType.Float:
                    return new KeyValuePair<string, EntityProperty>(name, new EntityProperty(value.ToObject<double>()));
                case JTokenType.Guid:
                    return new KeyValuePair<string, EntityProperty>(name, new EntityProperty(value.ToObject<Guid>()));
                case JTokenType.Integer:
                    return new KeyValuePair<string, EntityProperty>(name, new EntityProperty(value.ToObject<long>()));
                case JTokenType.String:
                    return new KeyValuePair<string, EntityProperty>(name, new EntityProperty(value.ToObject<string>()));
                default:
                    return null;
            }
        }

        /// <summary>
        /// Write to object.
        /// </summary>
        /// <param name="jObject">jobject.</param>
        /// <param name="property">property.</param>
        /// <param name="convertToTimeZone">convert to timezone.</param>
        /// <param name="fieldsAllowedForTimeZoneConversion">fields allowed for time zone conversion.</param>
        public static void WriteToJObject(JObject jObject, KeyValuePair<string, EntityProperty> property, string convertToTimeZone = null, List<string> fieldsAllowedForTimeZoneConversion = null)
        {
            if (jObject == null)
            {
                throw new ArgumentNullException(nameof(jObject));
            }

            switch (property.Value.PropertyType)
            {
                case EdmType.Binary:
                    jObject.Add(property.Key, new JValue(property.Value.BinaryValue));
                    return;
                case EdmType.Boolean:
                    jObject.Add(property.Key, new JValue(property.Value.BooleanValue));
                    return;
                case EdmType.DateTime:
                    if (!string.IsNullOrEmpty(convertToTimeZone) && property.Value.DateTime.HasValue && (fieldsAllowedForTimeZoneConversion == null || (fieldsAllowedForTimeZoneConversion != null && fieldsAllowedForTimeZoneConversion.Contains(property.Key))))
                    {
                        jObject.Add(property.Key, TimeZoneInfo.ConvertTimeFromUtc(property.Value.DateTime.Value, TimeZoneInfo.FindSystemTimeZoneById(convertToTimeZone)));
                    }
                    else
                    {
                        jObject.Add(property.Key, new JValue(property.Value.DateTime));
                    }

                    return;
                case EdmType.Double:
                    jObject.Add(property.Key, new JValue(property.Value.DoubleValue));
                    return;
                case EdmType.Guid:
                    jObject.Add(property.Key, new JValue(property.Value.GuidValue));
                    return;
                case EdmType.Int32:
                    jObject.Add(property.Key, new JValue(property.Value.Int32Value));
                    return;
                case EdmType.Int64:
                    jObject.Add(property.Key, new JValue(property.Value.Int64Value));
                    return;
                case EdmType.String:
                    jObject.Add(property.Key, new JValue(property.Value.StringValue));
                    return;
                default:
                    return;
            }
        }

        /// <summary>
        ///  Convert a Dynamic Table Entity to A POCO .NET Object.
        /// </summary>
        /// <typeparam name="TOutput">Desired Object Types.</typeparam>
        /// <param name="properties">Dictionary of Table Entity.</param>
        /// <param name="convertToTimeZone">Converts any datetime field in table entity to the required time zone.</param>
        /// <returns>.NET object.</returns>
        private static TOutput ConvertTo<TOutput>(IDictionary<string, EntityProperty> properties, string convertToTimeZone = null, List<string> fieldsAllowedForTimeZoneConversion = null)
        {
            var jobject = new JObject();
            foreach (var property in properties)
            {
                WriteToJObject(jobject, property, convertToTimeZone, fieldsAllowedForTimeZoneConversion);
            }

            return jobject.ToObject<TOutput>();
        }
    }
}
