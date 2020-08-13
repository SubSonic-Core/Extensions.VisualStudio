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
		public ParameterKey(string directiveId, string processorName, string parameterName)
		{
			this.ProcessorName = processorName ?? "";
			this.DirectiveId = directiveId ?? "";
			this.ParameterName = parameterName ?? "";
			unchecked
			{
				hashCode = this.ProcessorName.GetHashCode()
					^ this.DirectiveId.GetHashCode()
					^ this.ParameterName.GetHashCode();
			}
		}

		public string DirectiveId { get; }
		public string ProcessorName { get; }
		public string ParameterName { get; }

		private readonly int hashCode;

		public override bool Equals(object obj)
		{
			return obj is ParameterKey key && Equals(key);
		}

		public bool Equals(ParameterKey other)
		{
			return ProcessorName == other.ProcessorName && DirectiveId == other.DirectiveId && ParameterName == other.ParameterName;
		}

		public static bool operator ==(ParameterKey left, ParameterKey right)
        {
			return left.Equals(right);
        }

		public static bool operator !=(ParameterKey left, ParameterKey right)
		{
			return !left.Equals(right);
		}

		public override int GetHashCode()
		{
			return hashCode;
		}
	}
}
