using System.Runtime.InteropServices;

namespace SubSonic.Core.VisualStudio
{
    [Guid("8E741A52-B685-4261-BB96-E61B806AB2A0")]
    public interface ISubSonicCoreService
    {
        IConnectionManager ConnectionManager { get; }
    }
}
