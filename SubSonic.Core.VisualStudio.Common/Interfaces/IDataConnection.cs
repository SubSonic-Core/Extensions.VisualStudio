using System.Runtime.InteropServices;

namespace SubSonic.Core.VisualStudio
{
    [Guid("E4EC2E20-830A-47F6-B40B-F515EEBFCCD0")]
    public interface IDataConnection
    {
        string EncryptedConnectionString { get; }
        string SafeConnectionString { get; }
        string DisplayConnectionString { get; }
    }
}
