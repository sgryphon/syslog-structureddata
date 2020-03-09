using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Essential.Logging;
using Syslog.Collections;

namespace Syslog.StructuredData
{
    /// <summary>
    /// Holds structured data for logging as key-value properties
    /// </summary>
    public class StructuredData : IStructuredData
    {
        public const string IdProperty = "SD-ID";

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

        /// <summary>
        /// Gets the ID of the structured data set
        /// </summary>
        public string Id {
            get
            {
                return _id; 
            }
            [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
            set
            {
                if (_allProperties != null)
                {
                    throw new InvalidOperationException("Properties should not be added after the initial construction.");
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

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            EnsureAllProperties();
            return _allProperties!.GetEnumerator();
        }

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
                var prefix = string.IsNullOrWhiteSpace(_id) ? string.Empty : _id + ":";
                foreach (var kvp in _baseParameters)
                {
                    allProperties.Add(prefix + kvp.Key, kvp.Value);
                }

                if (!string.IsNullOrEmpty(_id))
                {
                    ((IDictionary<string, object>)allProperties)[IdProperty] = _id;
                }

                _allProperties = allProperties;
            }
        }
        
        private void EnsureToString()
        {
            if (_toString == null)
            {
                //EnsureMessageTemplate();
                EnsureAllProperties();

                var writer = new StringWriter();
                StructuredDataFormatter.FormatStructuredData(_id, _baseParameters, writer);
                _toString = writer.GetStringBuilder().ToString();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
