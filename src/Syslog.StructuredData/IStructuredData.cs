using System.Collections.Generic;

namespace Syslog.StructuredData
{
    public interface IStructuredData : IEnumerable<KeyValuePair<string, object>>
    {
        string Id { get; }
        IReadOnlyDictionary<string, object> Parameters { get; }
    }
}
