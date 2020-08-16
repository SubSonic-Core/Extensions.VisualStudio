using Mono.TextTemplating.CodeCompilation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SubSonic.Core.VisualStudio.Common
{
    public class RuntimeInfo
    {
        private RuntimeInfo(RuntimeKind kind, string directory, string error = null)
        {
            Kind = kind;
            RuntimeDirectory = directory ?? throw new ArgumentNullException(nameof(directory));
			Error = error;
        }

        public RuntimeKind Kind { get; }
        public string Error { get; }
        public string RuntimeDirectory { get; }
        public bool IsValid => Error == null;

        public static RuntimeInfo GetRuntime(RuntimeKind kind = RuntimeKind.Default)
        {
            var monoFx = GetMonoRuntime();
            if (monoFx.IsValid && (monoFx.Kind == kind || kind == RuntimeKind.Default))
            {
                return monoFx;
            }
            var netFx = GetNetFrameworkRuntime();
            if (netFx.IsValid && (netFx.Kind == kind || kind == RuntimeKind.Default))
            {
                return netFx;
            }
            var coreFx = GetDotNetCoreRuntime();
            if (coreFx.IsValid && (coreFx.Kind == kind || kind == RuntimeKind.Default))
            {
                return coreFx;
            }
            return new RuntimeInfo(RuntimeKind.Default, string.Empty, "Could not find any valid runtime");
        }

		public static IEnumerable<RuntimeInfo> GetAllValidRuntimes()
        {
			RuntimeInfo[] runtimes = new[]
			{
				GetMonoRuntime(),
				GetNetFrameworkRuntime(),
				GetDotNetCoreRuntime()
			};

			return runtimes.Where(x => x.IsValid);
        }

		private static RuntimeInfo GetMonoRuntime()
		{
			if (Type.GetType("Mono.Runtime") == null)
			{
				return new RuntimeInfo(RuntimeKind.Mono, string.Empty, "Current runtime is not Mono");
			}

			var runtimeDir = Path.GetDirectoryName(typeof(int).Assembly.Location);

			return new RuntimeInfo(RuntimeKind.Mono, runtimeDir);
		}

		private static RuntimeInfo GetNetFrameworkRuntime()
		{
			var runtimeDir = Path.GetDirectoryName(typeof(int).Assembly.Location);

			return new RuntimeInfo(RuntimeKind.NetFramework, runtimeDir);
		}

		private static RuntimeInfo GetDotNetCoreRuntime()
		{
			var dotnetRoot = FindDotNetRoot();

			if (dotnetRoot == null)
			{
				return new RuntimeInfo(RuntimeKind.NetCore, string.Empty, "Could not find .NET Core installation");
			}

			var runtimeDir = Utilities.FindHighestVersionedDirectory(Path.Combine(dotnetRoot, "shared", "Microsoft.NETCore.App"), d => File.Exists(Path.Combine(d, "System.Runtime.dll")));
			if (runtimeDir == null)
			{
				return new RuntimeInfo(RuntimeKind.NetCore, string.Empty, "Could not find System.Runtime.dll in any .NET shared runtime");
			}

			return new RuntimeInfo(RuntimeKind.NetCore, runtimeDir);
		}

		static string FindDotNetRoot()
		{
			string dotnetRoot;
			bool DotnetRootIsValid() => !string.IsNullOrEmpty(dotnetRoot) && (File.Exists(Path.Combine(dotnetRoot, "dotnet")) || File.Exists(Path.Combine(dotnetRoot, "dotnet.exe")));

			string FindInPath(string name) => (Environment.GetEnvironmentVariable("PATH") ?? "")
				.Split(new[] { Path.PathSeparator }, StringSplitOptions.RemoveEmptyEntries)
				.Select(p => Path.Combine(p, name))
				.FirstOrDefault(File.Exists);

			dotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT");
			if (DotnetRootIsValid())
			{
				return dotnetRoot;
			}

			// this should get us something like /usr/local/share/dotnet/shared/Microsoft.NETCore.App/2.1.2/System.Runtime.dll
			var runtimeDir = Path.GetDirectoryName(typeof(int).Assembly.Location);
			dotnetRoot = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(runtimeDir)));

			if (DotnetRootIsValid())
			{
				return dotnetRoot;
			}

			dotnetRoot = Path.GetDirectoryName(FindInPath(Path.DirectorySeparatorChar == '\\' ? "dotnet.exe" : "dotnet"));
			if (DotnetRootIsValid())
			{
				return dotnetRoot;
			}

			return null;
		}
	}
}
