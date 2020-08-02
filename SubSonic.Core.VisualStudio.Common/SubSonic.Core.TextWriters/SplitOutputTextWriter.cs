using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SubSonic.Core.TextWriters
{
	public class SplitOutputWriter : TextWriter
	{
		readonly TextWriter outputOneWriter;
		readonly TextWriter outputTwoWriter;

		public SplitOutputWriter(TextWriter outputOne, TextWriter outputTwo)
		{
			this.outputOneWriter = outputOne ?? throw new ArgumentNullException(nameof(outputOne));
			this.outputTwoWriter = outputTwo ?? throw new ArgumentNullException(nameof(outputTwo));
		}

		public override Encoding Encoding => Encoding.UTF8;

		public override void WriteLine()
		{
			outputOneWriter.WriteLine();
			outputTwoWriter.WriteLine();
		}

		public override void WriteLine(string value)
		{
			outputOneWriter.WriteLine(value);
			outputTwoWriter.WriteLine(value);
		}

		public override void Write(string value)
		{
			outputOneWriter.Write(value);
			outputTwoWriter.Write(value);
		}

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
