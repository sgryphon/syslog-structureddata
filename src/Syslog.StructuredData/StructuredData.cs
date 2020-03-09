using Syslog.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using Essential.Logging;

namespace Syslog.StructuredData
{
    /// <summary>
    /// Holds structured data for logging as key-value properties, along with a templated message.
    /// </summary>
    public class StructuredData : IStructuredData
    {
        private readonly string _id;
        
        /*
        public const string ExceptionProperty = "Exception";
        public const string MessageTemplateProperty = "MessageTemplate";
        */
        public const string IdProperty = "SD-ID";

        // Internally use ordered dictionary, to preserve the order passed in
        private OrderedDictionary<string, object>? _allProperties;
        private readonly OrderedDictionary<string, object> _baseParameters;
        
        /*
        string _messageTemplate;
        IList<string> _messageTemplateKeys;
        List<object> _templateValues;
        */
        
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

        /*
        /// <summary>
        /// Constructor. Creates structured data with the specified properties, message template, and (optional) override template values.
        /// </summary>
        /// <param name="parameters">The key-value properties to trace</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Optional values, assigned in sequence, to keys in the template</param>
        public StructuredData(IEnumerable<KeyValuePair<string, object>> parameters, string messageTemplate, params object[] templateValues)
            : this(parameters, null, messageTemplate, templateValues)
        {
        }

        /// <summary>
        /// Constructor. Creates structured data with the specified message template, and template values.
        /// </summary>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Values, assigned in sequence, to keys in the template</param>
        public StructuredData(string messageTemplate, params object[] templateValues)
            : this(null, null, messageTemplate, templateValues)
        {
        }

        /// <summary>
        /// Constructor. Creates structured data with the specified exception, message template, and template values.
        /// </summary>
        /// <param name="exception">The Exception to trace</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Values, assigned in sequence, to keys in the template</param>
        public StructuredData(Exception exception, string messageTemplate, params object[] templateValues)
            : this(null, exception, messageTemplate, templateValues)
        {
        }

        /// <summary>
        /// Constructor. Creates structured data with the specified properties, exception, message template, and (optional) override template values.
        /// </summary>
        /// <param name="parameters">The key-value properties to trace</param>
        /// <param name="exception">The Exception to trace</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Optional values, assigned in sequence, to keys in the template</param>
        /// <remarks>
        /// <para>
        /// Note that the relationship between messageTemplate and templateValues is flexible.
        /// </para>
        /// <para>
        /// The IStructuredData.Properties dictionary is built from the properties parameter, if present, 
        /// with the exception parameter, if present, overriding the "Exception" property.
        /// </para>
        /// <para>
        /// Items in templateValues, if any, are then added to the properties using the matching key in sequence from
        /// messageTemplate, overriding any existing value in properties. Note that templateValues are not needed, and 
        /// there is no error if there are less values than messageTemplate keys. If there are more values than keys, 
        /// then they are added as "Extra1", "Extra2", etc.
        /// </para>
        /// </remarks>
        public StructuredData(IEnumerable<KeyValuePair<string, object>> parameters, Exception exception, string messageTemplate, params object[] templateValues)
        {
            if (parameters == null)
            {
                _baseProperties = new OrderedDictionary<string, object>();
            }
            else
            {
                _baseProperties = new OrderedDictionary<string, object>(parameters);
            }
            _exception = exception;
            _messageTemplate = messageTemplate;
            if (templateValues == null)
            {
                _templateValues = new List<object>();
            }
            else
            {
                _templateValues = new List<object>(templateValues);
            }
        }
        */
        
        public IEnumerable<KeyValuePair<string, object>> BaseParameters
        {
            get { return _baseParameters; }
        }
        
        /*
        public int Count
        {
            get
            {
                EnsureAllProperties();
                return _allProperties!.Count;
            }
        }
        */

        // public Exception Exception {
        //     get
        //     {
        //         if (_exception == null)
        //         {
        //             // Could be from either base properties or template values
        //             EnsureAllProperties();
        //             object exceptionFromAllProperties;
        //             if (_allProperties.TryGetValue(ExceptionProperty, out exceptionFromAllProperties))
        //             {
        //                 if (exceptionFromAllProperties is Exception)
        //                 {
        //                     return (Exception)exceptionFromAllProperties;
        //                 }
        //             }
        //         }
        //         return _exception;
        //     }
        // }
        //
        // public string MessageTemplate {
        //     get
        //     {
        //         EnsureMessageTemplate();
        //         return _messageTemplate;
        //     }
        // }
        //
        // public IEnumerable<object> TemplateValues { get { return _templateValues; } }

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

        /*
        public bool ContainsKey(string key)
        {
            EnsureAllProperties();
            return _allParameters.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            EnsureAllProperties();
            return _allParameters.GetEnumerator();
        }
        */

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

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /*
        public bool TryGetValue(string key, out object value)
        {
            EnsureAllProperties();
            return _allParameters.TryGetValue(key, out value);
        }
        */

        /*
        #region IDictionary

        ICollection<string> IDictionary<string, object>.Keys
        {
            get
            {
                EnsureAllProperties();
                return _allParameters.Keys;
            }
        }

        ICollection<object> IDictionary<string, object>.Values
        {
            get
            {
                EnsureAllProperties();
                return _allParameters.Values;
            }
        }

        bool ICollection<KeyValuePair<string, object>>.IsReadOnly
        {
            get { return true; }
        }


        void IDictionary<string, object>.Add(string key, object value)
        {
            throw new NotSupportedException();
        }

        bool IDictionary<string, object>.Remove(string key)
        {
            throw new NotSupportedException();
        }

        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
        {
            throw new NotSupportedException();
        }

        void ICollection<KeyValuePair<string, object>>.Clear()
        {
            throw new NotSupportedException();
        }

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            EnsureAllProperties();
            return _allParameters.Contains(item);
        }

        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            EnsureAllProperties();
            _allParameters.CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            throw new NotSupportedException();
        }
        */

        /*
        IEnumerator IEnumerable.GetEnumerator()
        {
            EnsureAllProperties();
            return _allParameters.GetEnumerator();
        }
        */


        private void EnsureAllProperties()
        {
            if (_allProperties == null)
            {
                var allProperties = new OrderedDictionary<string, object>();
                // Get message template, or if not there, see if there is one in base properties
                //EnsureMessageTemplate();
                
                // Start with base properties, with keys prefixed by the SD-ID
                var prefix = string.IsNullOrWhiteSpace(_id) ? string.Empty : _id + ":";
                foreach (var kvp in _baseParameters)
                {
                    allProperties.Add(prefix + kvp.Key, kvp.Value);
                }
                
                /*
                // If have a template, extract keys
                if (_messageTemplate != null)
                {
                    var keyExtractor = new MessageTemplateKeyExtractor(_messageTemplate);
                    _messageTemplateKeys = keyExtractor.GetKeys();
                }
                else
                {
                    _messageTemplateKeys = new List<string>();
                }
                
                // Get properties from template values
                if (_templateValues != null && _templateValues.Count > 0)
                {
                    var extraCount = 0;
                    for (var index = 0; index < _templateValues.Count; index++)
                    {
                        if (index < _messageTemplateKeys.Count)
                        {
                            ((IDictionary<string,object>)allProperties)[_messageTemplateKeys[index]] = _templateValues[index];
                        }
                        else
                        {
                            extraCount++;
                            ((IDictionary<string, object>)allProperties)[string.Format("Extra{0}", extraCount)] = _templateValues[index];
                        }
                    }
                }
                
                // Set the template actually used (will overwrite template values)
                if (_messageTemplate != null)
                {
                    ((IDictionary<string, object>)allProperties)[MessageTemplateProperty] = _messageTemplate;
                }
                
                // Set the exception (will overwrite)
                if (_exception != null)
                {
                    ((IDictionary<string, object>)allProperties)[ExceptionProperty] = _exception;
                }
                
                */

                if (!string.IsNullOrEmpty(_id))
                {
                    ((IDictionary<string, object>)allProperties)[IdProperty] = _id;
                }

                _allProperties = allProperties;
            }
        }

        /*
        private void EnsureMessageTemplate()
        {
            if (_messageTemplate == null)
            {
                object _basePropertiesMessageTemplate;
                if (_baseProperties.TryGetValue(MessageTemplateProperty, out _basePropertiesMessageTemplate))
                {
                    if (_basePropertiesMessageTemplate is string)
                    {
                        _messageTemplate = (string)_basePropertiesMessageTemplate;
                    }
                }
            }
        }
        */
        
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

        /*
        private bool GetValue(string name, out object value)
        {
            if (_allParameters.TryGetValue(name, out value))
            {
                if (name.StartsWith("@"))
                {
                    value = StructuredPropertyFormatter.DestructureObject(value);
                }
            }
            else
            {
                value = "{" + name + "}";
            }
            return true;
        }

        class MessageTemplateKeyExtractor
        {
            string _messageTemplate;
            IList<string> _keys;

            public MessageTemplateKeyExtractor(string messageTemplate)
            {
                _messageTemplate = messageTemplate;
            }

            public IList<string> GetKeys()
            {
                if (_keys == null)
                {
                    _keys = new List<string>();
                    var dummy = StringTemplate.Format(_messageTemplate, GetValue);
                }
                return _keys;
            }

            private bool GetValue(string name, out object value)
            {
                _keys.Add(name);
                value = null;
                return true;
            }
        }
        */

        public string Id { get { return _id; } }
        
        public IDictionary<string, object> Parameters { get { return _baseParameters; } }
    }
}
