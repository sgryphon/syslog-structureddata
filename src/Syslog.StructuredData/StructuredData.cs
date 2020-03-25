using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Syslog.Collections;

namespace Syslog
{
    /// <summary>
    /// Holds structured data for logging as key-value properties
    /// </summary>
    public class StructuredData : IStructuredData
    {
        public const string IdKey = "SD-ID";
        public const string IdPrefixSeparator = ":";

        // Internally use ordered dictionary, to preserve the order passed in
        private OrderedDictionary<string, object>? _allProperties;
        private readonly OrderedDictionary<string, object> _baseParameters;
        private string _id;

        private string? _toString;

        /// <summary>
        /// Constructor. Creates empty structured data, to support dictionary initializer syntax.
        /// </summary>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public StructuredData()
            : this(string.Empty, new KeyValuePair<string, object>[0])
        {
        }

        /// <summary>
        /// Constructor. Creates structured data with the specified structured data parameters and an empty ID.
        /// </summary>
        /// <param name="parameters">The key-value parameters to trace</param>
        public StructuredData(IEnumerable<KeyValuePair<string, object>> parameters)
            : this(string.Empty, parameters)
        {
        }

        /// <summary>
        /// Constructor. Creates structured data with the specified parameters.
        /// </summary>
        /// <param name="id">The structured data ID (name); RFC 5424 specifies the format 'name@private-enterprise-number' for custom values.</param>
        /// <param name="parameters">The key-value parameters to trace</param>
        public StructuredData(string id, IEnumerable<KeyValuePair<string, object>> parameters)
        {
            _id = id;
            _baseParameters = new OrderedDictionary<string, object>(parameters);
        }

        int IReadOnlyCollection<KeyValuePair<string, object>>.Count
        {
            get
            {
                EnsureAllProperties();
                return _allProperties!.Count;
            }
        }

        /// <summary>
        /// Gets the ID of the structured data set
        /// </summary>
        public string Id
        {
            get
            {
                return _id;
            }
            [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
            set
            {
                if (_allProperties != null)
                {
                    throw new InvalidOperationException(
                        "Properties should not be added after the initial construction.");
                }

                _id = value;
            }
        }

        /// <summary>
        /// Gets the parameters of the structured data
        /// </summary>
        public IReadOnlyDictionary<string, object> Parameters { get { return _baseParameters; } }

        /// <summary>
        /// Gets the value associated with the provided key. Set is provided to support dictionary initializer syntax, and will fail after initial data setup.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The setter will thrown an exception if called after initial setup, i.e. after any values have been read from the object.
        /// </exception>
        public object? this[string key]
        {
            get
            {
                EnsureAllProperties();
                return ((IDictionary<string, object>)_allProperties!)[key];
            }
            [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
            set
            {
                Add(key, value);
            }
        }

        KeyValuePair<string, object> IReadOnlyList<KeyValuePair<string, object>>.this[int index]
        {
            get
            {
                EnsureAllProperties();
                return _allProperties![index];
            }
        }

        /// <summary>
        /// Adds the key-value pair to the base properties, to support dictionary initializer syntax, and will fail after initial data setup.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when Add is called after initial setup, i.e. after any values have been read from the object.
        /// </exception>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public void Add(string key, object? value)
        {
            if (_allProperties != null)
            {
                throw new InvalidOperationException("Properties should not be added after the initial construction.");
            }

            _baseParameters.Add(key, value!);
        }

        /// <summary>
        /// Formats the ID and Parameter values of the provided IStructuredData in RFC 5424 format, e.g. [sd-id param1="value1" param2="value2"]
        /// </summary>
        public static string Format(IStructuredData structuredData)
        {
            return Format(structuredData.Id, structuredData.Parameters);
        }

        /// <summary>
        /// Formats the provided ID and Parameter values as RFC 5424 structured data, e.g. [sd-id param1="value1" param2="value2"]
        /// </summary>
        /// <param name="id">The structured data ID (name); RFC 5424 specifies the format 'name@private-enterprise-number' for custom values.</param>
        /// <param name="parameters">The key-value parameters to trace</param>
        public static string Format(string id, IEnumerable<KeyValuePair<string, object>> parameters)
        {
            var writer = new StringWriter();
            StructuredDataFormatter.FormatStructuredData(id, parameters, writer);
            return writer.GetStringBuilder().ToString();
        }

        /// <summary>
        /// Extracts "SD-ID" as the structured data ID, removes corresponding prefixes from other keys, then formats as RFC 5424 structured data.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For example, if the values contain { ["SD-ID"] = "a", ["a:b"] = 1, ["c"] = "x" }, then the ID is "a",
        /// and the second key becomes "b" to give a formatted result of [a b="1" c="x"].
        /// </para>
        /// </remarks>
        public static string Format(IEnumerable<KeyValuePair<string, object>> values)
        {
            if (values is IStructuredData structuredData)
            {
                return Format(structuredData.Id, structuredData.Parameters);
            }
            else
            {
                GetIdAndParametersFromList(values, out var id, out var parameters);
                return Format(id, parameters);
            }
        }

        /// <summary>
        /// Formats a single parameter name and value in RFC 5424 format, with an empty ID, e.g. [- paramName="parameterValue"]
        /// </summary>
        public static string Format(string parameterName, object parameterValue)
        {
            return Format(string.Empty, parameterName, parameterValue);
        }

        /// <summary>
        /// Formats the ID and a single parameter name and value in RFC 5424 format, e.g. [id paramName="parameterValue"]
        /// </summary>
        public static string Format(string id, string parameterName, object parameterValue)
        {
            var parameters = new[] {new KeyValuePair<string, object>(parameterName, parameterValue)};
            return Format(id, parameters);
        }

        /// <summary>
        /// Creates a StructuredData by extracting any "SD-ID" as the ID, and removing the corresponding prefix from other keys.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For example, if the values contain { ["SD-ID"] = "a", ["a:b"] = 1, ["c"] = "x" }, then the ID is "a",
        /// and the second key becomes "b" to give a StructuredData object with Id = "a", and Parameters containing
        /// two keys "b" and "c".
        /// </para>
        /// </remarks>
        public static IStructuredData From(IEnumerable<KeyValuePair<string, object>> values)
        {
            if (values is IStructuredData structuredData)
            {
                return structuredData;
            }
            else
            {
                GetIdAndParametersFromList(values, out var id, out var parameters);
                return new StructuredData(id, parameters);
            }
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            EnsureAllProperties();
            return _allProperties!.GetEnumerator();
        }

        /// <summary>
        /// Formats the structured data in RFC 5424 format, e.g. [sd-id param1="value1" param2="value2"]
        /// </summary>
        public override string ToString()
        {
            EnsureToString();
            return _toString!;
        }

        private void EnsureAllProperties()
        {
            if (_allProperties == null)
            {
                var allProperties = new OrderedDictionary<string, object>();

                // Start with base properties, with keys prefixed by the SD-ID
                var prefix = string.IsNullOrWhiteSpace(_id) ? string.Empty : _id + IdPrefixSeparator;
                foreach (var kvp in _baseParameters)
                {
                    allProperties.Add(prefix + kvp.Key, kvp.Value);
                }

                if (!string.IsNullOrEmpty(_id))
                {
                    ((IDictionary<string, object>)allProperties)[IdKey] = _id;
                }

                _allProperties = allProperties;
            }
        }

        private void EnsureToString()
        {
            if (_toString == null)
            {
                //EnsureMessageTemplate();

                EnsureAllProperties(); // Although _allProperties isn't needed, this locks the object and prevents further setters
                _toString = Format(this);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private static void GetIdAndParametersFromList(IEnumerable<KeyValuePair<string, object>> values, out string id,
            out IEnumerable<KeyValuePair<string, object>> parameters)
        {
            id = values.Where(kvp => string.Equals(kvp.Key, IdKey, StringComparison.OrdinalIgnoreCase))
                     .Select(kvp => kvp.Value.ToString())
                     .SingleOrDefault()
                 ?? string.Empty;

            var prefix = string.IsNullOrWhiteSpace(id) ? string.Empty : id + IdPrefixSeparator;

            parameters = values.Where(kvp => !string.Equals(kvp.Key, IdKey, StringComparison.OrdinalIgnoreCase))
                .Select(kvp =>
                {
                    if (prefix != string.Empty && kvp.Key.StartsWith(prefix))
                    {
                        return new KeyValuePair<string, object>(kvp.Key.Substring(prefix.Length), kvp.Value);
                    }
                    else
                    {
                        return kvp;
                    }
                });
        }
    }
}
