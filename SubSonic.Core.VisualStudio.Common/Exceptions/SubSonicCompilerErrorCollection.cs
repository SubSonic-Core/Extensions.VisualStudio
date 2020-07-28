using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom.Compiler;

namespace SubSonic.Core.VisualStudio.Common.Exceptions
{
    public sealed class SubSonicCompilerErrorCollection
        : CompilerErrorCollection
    {
        public SubSonicCompilerErrorCollection()
            : base()
        {

        }

        public SubSonicCompilerErrorCollection(SubSonicCompilerError[] errors)
            : base(errors)
        {

        }

        public SubSonicCompilerErrorCollection(SubSonicCompilerErrorCollection collection)
            : base(collection)
        {

        }
    }
}
