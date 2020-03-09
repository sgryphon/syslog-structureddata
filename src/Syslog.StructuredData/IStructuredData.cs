using System.Collections.Generic;

namespace Syslog.StructuredData
{
    public interface IStructuredData : IEnumerable<KeyValuePair<string, object>>
    {
        string Id { get; }
        IDictionary<string, object> Parameters { get; }
    }
}
