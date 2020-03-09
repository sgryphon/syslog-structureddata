using System.Collections.Generic;

namespace Syslog
{
    public interface IStructuredData : IEnumerable<KeyValuePair<string, object>>
    {
        string Id { get; }
        IReadOnlyDictionary<string, object> Parameters { get; }
    }
}
