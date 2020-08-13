using System;

namespace SubSonic.Core.VisualStudio.Host
{
    public delegate object GetHostOptionEventHandler(object sender, GetHostOptionEventArgs args);

	public class GetHostOptionEventArgs 
		: EventArgs
	{
		public GetHostOptionEventArgs(string optionName)
        {
			OptionName = optionName ?? throw new ArgumentNullException(nameof(optionName)); 
        }

		public string OptionName { get; }
	}

	public delegate string ResolveAssemblyReferenceEventHandler(object sender, ResolveAssemblyReferenceEventArgs args);

	public class ResolveAssemblyReferenceEventArgs
		: EventArgs
	{
		public ResolveAssemblyReferenceEventArgs(string assemblyReference)
		{
			AssemblyReference = assemblyReference ?? throw new ArgumentNullException(nameof(assemblyReference));
		}

		public string AssemblyReference { get; }
	}

	public delegate string ExpandAllVariablesEventHandler(object sender, ExpandAllVariablesEventArgs args);

	public class ExpandAllVariablesEventArgs
		: EventArgs
	{
		public ExpandAllVariablesEventArgs(string filePath)
		{
			FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
		}

		public string FilePath { get; }
	}

	public delegate string ResolveParameterValueEventHandler(object sender, ResolveParameterValueEventArgs args);

	public class ResolveParameterValueEventArgs
		: EventArgs
    {
		public ResolveParameterValueEventArgs(ParameterKey parameterKey)
        {
			this.ParameterKey = parameterKey;
        }

		public ParameterKey ParameterKey { get; }
	}

	public delegate Type ResolveDirectiveProcessorEventHandler(object sender, ResolveDirectiveProcessorEventArgs args);

	public class ResolveDirectiveProcessorEventArgs
		: EventArgs
    {
		public ResolveDirectiveProcessorEventArgs(string processorName)
        {
			this.ProcessorName = processorName ?? throw new ArgumentNullException(nameof(processorName));
        }

		public string ProcessorName { get; }
	}
}