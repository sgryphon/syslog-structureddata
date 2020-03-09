using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Essential.Logging
{
    internal class StructuredDataFormatter
    {
        static readonly IDictionary<Type, Action<object, TextWriter>> LiteralWriters;

        static StructuredDataFormatter()
        {
            LiteralWriters = new Dictionary<Type, Action<object, TextWriter>>
            {
                { typeof(bool), (v, w) => WriteBoolean((bool)v, w) },
                //{ typeof(char), (v, w) => WriteEscapedString(((char)v).ToString(), w) },
                { typeof(byte), (v, w) => WriteByte((byte)v, w) },
                { typeof(byte[]), (v, w) => WriteByteArray((byte[])v, w) },
                { typeof(sbyte), WriteToString },
                { typeof(short), WriteToString },
                { typeof(ushort), WriteToString },
                { typeof(int), WriteToString },
                { typeof(uint), WriteToString },
                { typeof(long), WriteToString },
                { typeof(ulong), WriteToString },
                { typeof(float), WriteToString },
                { typeof(double), WriteToString },
                { typeof(decimal), WriteToString },
                { typeof(Guid), WriteToString },
                { typeof(TimeSpan), WriteToString },
                //{ typeof(string), (v, w) => WriteEscapedString((string)v, w) },
                { typeof(DateTime), (v, w) => WriteDateTime((DateTime)v, w) },
                { typeof(DateTimeOffset), (v, w) => WriteDateTimeOffset((DateTimeOffset)v, w) },
            };
        }

        // public static string DestructureObject(object obj)
        // {
        //     var writer = new StringWriter();
        //     DestructurePropertyValue(obj, writer, 0, 0);
        //     return writer.ToString();
        // }

        // From RFC 5424 - The Syslog Protocol
        //
        // STRUCTURED-DATA = NILVALUE / 1*SD-ELEMENT
        // SD-ELEMENT      = "[" SD-ID *(SP SD-PARAM) "]"
        // SD-PARAM        = PARAM-NAME "=" %d34 PARAM-VALUE %d34
        // SD-ID           = SD-NAME
        // PARAM-NAME      = SD-NAME
        // PARAM-VALUE     = UTF-8-STRING ; characters '"', '\' and
        //                                ; ']' MUST be escaped.
        // SD-NAME         = 1*32PRINTUSASCII
        //                   ; except '=', SP, ']', %d34 (")
        // UTF-8-STRING    = *OCTET ; UTF-8 string as specified
        //                   ; in RFC 3629
        // OCTET           = %d00-255
        // SP              = %d32
        // PRINTUSASCII    = %d33-126         
        public static void FormatStructuredData(string id, IEnumerable<KeyValuePair<string, object>> parameters, TextWriter output)
        {
            output.Write('[');
 
            // SD-ID
            if (string.IsNullOrEmpty(id))
            {
                output.Write('-');
            }
            else
            {
                WriteName(id, output);
            }

            // SD-PARAMs
            foreach (var kvp in parameters)
            {
                WriteParameter(kvp.Key, kvp.Value, output, 0, 0);
            }
            
            output.Write(']');
        }

        static void DestructurePropertyValue(object obj, TextWriter output, int arrayCount, int destructureCount)
        {
            if (obj == null)
            {
                output.Write("null");
                return;
            }

            var typeInfo = obj.GetType().GetTypeInfo();
            
            //var publicProperties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
            var readableProperties = typeInfo.DeclaredProperties.Where(x => x.CanRead);
            output.Write("{");
            destructureCount = destructureCount + 1;
            var index = 0;
            foreach (var propertyInfo in readableProperties)
            {
                if (index > 0)
                {
                    output.Write(" ");
                }
                var propertyValue = propertyInfo.GetValue(obj, null);
                WriteName(propertyInfo.Name, output);
                output.Write("=");
                WritePropertyValue(propertyValue, output, arrayCount, destructureCount, '\'');
                index++;
            }
            output.Write("}");
        }

        static void WriteArray(IList array, TextWriter output, int arrayCount, int destructureCount)
        {
            // The standard array.ToString() isn't very interesting, so render the elements
            output.Write("(");
            arrayCount = arrayCount + 1;
            for (var index = 0; index < array.Count; index++)
            {
                if (index > 0)
                {
                    output.Write(",");
                }
                var value = array[index];
                WritePropertyValue(value, output, arrayCount, destructureCount, '\'');
            }
            output.Write(")");
        }

        static void WriteDictionary(IDictionary<string, object> dictionary, TextWriter output, int arrayCount, int destructureCount)
        {
            // The standard dictionary.ToString() isn't very interesting, so render the key-value pairs
            output.Write("(");
            destructureCount = destructureCount + 1;
            var index = 0;
            foreach (var kvp in dictionary)
            {
                if (index > 0)
                {
                    output.Write(" ");
                }
                WriteName(kvp.Key, output);
                output.Write("=");
                WritePropertyValue(kvp.Value, output, arrayCount, destructureCount, '\'');
                index++;
            }
            output.Write(")");
        }

        static void WriteBoolean(bool value, TextWriter output)
        {
            output.Write(value ? "true" : "false");
        }

        static void WriteByte(byte value, TextWriter output)
        {
            output.Write("0x");
            output.Write(value.ToString("X2"));
        }

        static void WriteByteArray(byte[] value, TextWriter output)
        {
            output.Write("0x");
            foreach (var b in value)
            {
                output.Write(b.ToString("X2"));
            }
        }

        static void WriteDateTime(DateTime value, TextWriter output)
        {
            if (value.TimeOfDay.Equals(TimeSpan.Zero))
            {
                output.Write("{0:yyyy'-'MM'-'dd}", value);
            }
            else
            {
                output.Write(value.ToString("s"));
            }
        }

        static void WriteDateTimeOffset(DateTimeOffset value, TextWriter output)
        {
            output.Write("{0:yyyy'-'MM'-'dd'T'HH':'mm':'ss.ffffffzzz}", value);
        }

        static void WriteParameter(string name, object value, TextWriter output, int arrayCount, int destructureCount)
        {
            output.Write(' ');
            WriteName(name, output);
            output.Write("=\"");
            if (name.StartsWith("@"))
            {
                DestructurePropertyValue(value, output, arrayCount, destructureCount);
            }
            else
            {
                WritePropertyValue(value, output, arrayCount, destructureCount, null);
            }
            output.Write('"');
        }
        
        static void WriteName(string name, TextWriter output)
        {
            // Callers should not use names containing invalid characters,
            // but if they do, convert to "_", "\0", "\xNN" or "\uNNNN"
            foreach (char c in name.Cast<char>())
            {
                if (c == ' ')
                {
                    output.Write('_');
                }
                else if (c == '\0')
                {
                    output.Write("\\0");
                }
                else if (c > '\xff')
                {
                    output.Write("\\u{0:x4}", (int)c);
                }
                else if (c < '\x21' || c > '\x7e' || c == '=' || c == ']' || c == '"')
                {
                    output.Write("\\x{0:x2}", (int)c);
                }
                else
                {
                    output.Write(c);
                }
            }
        }

        static void WritePropertyValue(object value, TextWriter output, int arrayCount, int destructureCount, char? stringDelimiter)
        {
            if (value is null)
            {
                output.Write("null");
                return;
            }
            if (LiteralWriters.TryGetValue(value.GetType(), out var writer))
            {
                writer(value, output);
                return;
            }
            if (destructureCount < 1 && arrayCount < 1)
            {
                if (value is IDictionary<string, object>)
                {
                    WriteDictionary((IDictionary<string, object>)value, output, arrayCount, destructureCount);
                    return;
                }
            }
            if (arrayCount < 1)
            {
                if (value is IList)
                {
                    WriteArray((IList)value, output, arrayCount, destructureCount);
                    return;
                }
            }
            WriteEscapedString(value.ToString(), output, stringDelimiter);
        }

        static void WriteEscapedString(string value, TextWriter output, char? stringDelimiter)
        {
            if (stringDelimiter != null)
            {
                output.Write(stringDelimiter);
            }
            foreach (var c in value.Cast<char>())
            {
                // characters '"', '\' and ']' MUST be escaped.
                if (c == '"' || c == '\\' || c == ']')
                {
                    output.Write('\\');
                    output.Write(c);
                }
                else if (c <= '\x1f' || (c >= '\x7f' && c <= '\x9f'))
                {
                    // The syslog application MAY modify messages
                    // containing control characters (e.g., by changing an octet with value
                    // 0 (USASCII NUL) to the four characters "#000")
                    // => Use '\xNN', which is an invalid sequence, so the '\' is treated as a literal. 
                    output.Write("\\x{0:x2}", (int)c);
                }
                else if (stringDelimiter != null && c == stringDelimiter)
                {    
                    output.Write('\\');
                    output.Write(c);
                }
                else
                {
                    output.Write(c);
                }
            }
            if (stringDelimiter != null)
            {
                output.Write(stringDelimiter);
            }
        }

        private static void WriteToString(object number, TextWriter output)
        {
            output.Write(number.ToString());
        }

        private delegate void Action<T1, T2>(T1 a, T2 b);
    }
}
