using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextTemplating;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Core.VisualStudio.CustomTools
{
    public partial class SubSonicTemplatingFileGenerator
        : ITextTemplatingEngineHost
    {
        private readonly ITextTemplatingEngineHost host;
        public SubSonicTemplatingFileGenerator()
        {
            if (Package.GetGlobalService(typeof(STextTemplating)) is ITextTemplatingEngineHost engine)
            {
                host = engine;
            }
        }
        public IList<string> StandardAssemblyReferences => host.StandardAssemblyReferences;

        public IList<string> StandardImports => host.StandardImports;

        public string TemplateFile => host.TemplateFile;

        public object GetHostOption(string optionName)
        {
            return host.GetHostOption(optionName);
        }

        public bool LoadIncludeText(string requestFileName, out string content, out string location)
        {
            return host.LoadIncludeText(requestFileName, out content, out location);
        }

        public void LogErrors(CompilerErrorCollection errors)
        {
            host.LogErrors(errors);
        }

        public AppDomain ProvideTemplatingAppDomain(string content)
        {
            return host.ProvideTemplatingAppDomain(content);
        }

        public string ResolveAssemblyReference(string assemblyReference)
        {
            return ResolveAssemblyReference(assemblyReference);
        }

        public Type ResolveDirectiveProcessor(string processorName)
        {
            return host.ResolveDirectiveProcessor(processorName);
        }

        public string ResolveParameterValue(string directiveId, string processorName, string parameterName)
        {
            return host.ResolveParameterValue(directiveId, processorName, parameterName);
        }

        public string ResolvePath(string path)
        {
            return host.ResolvePath(path);
        }

        public void SetFileExtension(string extension)
        {
            host.SetFileExtension(extension);
        }

        public void SetOutputEncoding(Encoding encoding, bool fromOutputDirective)
        {
            host.SetOutputEncoding(encoding, fromOutputDirective);
        }
    }
}
