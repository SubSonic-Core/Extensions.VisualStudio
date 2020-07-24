using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using SubSonic.Core.VisualStudio.Templating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SubSonic.Core.VisualStudio.Services
{
    public partial class SubSonicCoreService
        : IDebugTextTemplating
        , ITextTemplating
    {
        private static readonly Regex directiveParsingRegex = new Regex("template.*\\slanguage\\s*=\\s*\"(?<pvalue>.*?)(?<=[^\\\\](\\\\\\\\)*)\"", RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        #region IDebugTextTemplating
        public void DebugTemplateAsync(string inputFilename, string content, ITextTemplatingCallback callback, object hierarchy)
        {
            DebugTemplating.DebugTemplateAsync(inputFilename, content, callback, hierarchy);
        }

        public string ProcessTemplate(string inputFile, string content, ITextTemplatingCallback callback = null, object hierarchy = null)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            Hierarchy = hierarchy;
            Callback = callback;
            InputFile = inputFile;

            Host.SetFileExtension(SearchForLanguage(content, "C#") ? ".cs" : ".vb");

            string result = Engine.ProcessTemplate(content, Host);

            // TextTemplatingService does some extra stuff here which I can not call at this time.

            if (SearchForLanguage(content, "C#"))
            {
                SqmFacade.T4PreprocessTemplateCS();
            }
            else if (SearchForLanguage(content, "VB"))
            {
                SqmFacade.T4PreprocessTemplateVB();
            }

            return result;
        }

        public string PreprocessTemplate(string inputFile, string content, ITextTemplatingCallback callback, string className, string classNamespace, out string[] references)
        {
            return templating.PreprocessTemplate(inputFile, content, callback, className, classNamespace, out references);
        }

        public void BeginErrorSession()
        {
            DebugTemplating.BeginErrorSession();
        }

        public bool EndErrorSession()
        {
            return DebugTemplating.EndErrorSession();
        }
        #endregion

        private bool SearchForLanguage(string templateContent, string language)
        {
            bool flag = false;
            MatchCollection matchs = directiveParsingRegex.Matches(templateContent);
            if (matchs.Count > 0)
            {
                flag = string.Equals(matchs[0].Groups["pvalue"].Value, language, StringComparison.OrdinalIgnoreCase);
            }
            return flag;
        }
    }
}
