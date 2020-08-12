using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Core.VisualStudio.Host
{
	[Serializable]
	public struct ParameterKey
			: IEquatable<ParameterKey>
	{
		public ParameterKey(string processorName, string directiveName, string parameterName)
		{
			this.processorName = processorName ?? "";
			this.directiveName = directiveName ?? "";
			this.parameterName = parameterName ?? "";
			unchecked
			{
				hashCode = this.processorName.GetHashCode()
					^ this.directiveName.GetHashCode()
					^ this.parameterName.GetHashCode();
			}
		}

        private readonly string processorName, directiveName, parameterName;
        private readonly int hashCode;

		public override bool Equals(object obj)
		{
			return obj is ParameterKey key && Equals(key);
		}

		public bool Equals(ParameterKey other)
		{
			return processorName == other.processorName && directiveName == other.directiveName && parameterName == other.parameterName;
		}

		public override int GetHashCode()
		{
			return hashCode;
		}
	}
}
