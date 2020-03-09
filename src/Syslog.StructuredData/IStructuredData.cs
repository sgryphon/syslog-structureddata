using System.Collections.Generic;

namespace Syslog
{
    public interface IStructuredData : IReadOnlyList<KeyValuePair<string, object>>
    {
        string Id { get; }
        IReadOnlyDictionary<string, object> Parameters { get; }
    }
}
