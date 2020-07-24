using System.Runtime.InteropServices;

namespace SubSonic.Core.VisualStudio
{
    [Guid("2059C9C2-BF28-4788-9707-7073745A31A0")]
    public interface IConnectionManager
    {
        IDataConnection this[string key] { get; }

        void Add(string key, IDataConnection connection);
    }
}
