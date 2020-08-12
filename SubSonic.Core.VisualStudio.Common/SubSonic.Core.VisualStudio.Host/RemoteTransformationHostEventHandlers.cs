using System;

namespace SubSonic.Core.VisualStudio.Host
{
	public delegate object GetHostOptionEventHandler(object sender, GetHostOptionEventArgs args);

	public class GetHostOptionEventArgs 
		: EventArgs
	{
		public GetHostOptionEventArgs(string optionName)
        {
			OptionName = optionName;
        }

		public string OptionName { get; }
	}

	public delegate string ResolveAssemblyReferenceEventHandler(object sender, ResolveAssemblyReferenceEventArgs args);

	public class ResolveAssemblyReferenceEventArgs
		: EventArgs
	{
		public ResolveAssemblyReferenceEventArgs(string assemblyReference)
		{
			AssemblyReference = assemblyReference;
		}

		public string AssemblyReference { get; }
	}

	public delegate string ExpandAllVariablesEventHandler(object sender, ExpandAllVariablesEventArgs args);

	public class ExpandAllVariablesEventArgs
		: EventArgs
	{
		public ExpandAllVariablesEventArgs(string filePath)
		{
			FilePath = filePath;
		}

		public string FilePath { get; }
	}
}