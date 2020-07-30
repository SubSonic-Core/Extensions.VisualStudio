using EnvDTE;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SubSonic.Core.VisualStudio.Utilities
{
    public static class PathHelper
    {
        private static Regex vsMacroAndPropertyRegEx = new Regex(@" \$\(  (?<name>\w+) \) ", RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        public static async Task<string> ExpandVsMacroVariablesAsync(string path, IVsHierarchy hierarchy)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (hierarchy is IVsBuildMacroInfo buildMacroInfo)
            {
                MatchEvaluator evaluator = delegate (Match m)
                {
                    ThreadHelper.ThrowIfNotOnUIThread();

                    Group group = m.Groups["name"];

                    string value = null;

                    bool success = !group.Success || (!ErrorHandler.Succeeded(buildMacroInfo.GetBuildMacroValue(group.Value, out value)) || string.IsNullOrEmpty(value));

                    return success ? m.Value : value;
                };

                path = vsMacroAndPropertyRegEx.Replace(path, evaluator);
            }

            return path;
        }

        public static async Task<string> ExpandProjectPropertiesAsync(string path, IVsHierarchy hierarchy, IServiceProvider provider)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (hierarchy is IVsBuildPropertyStorage propertyStorage)
            {
                Lazy<string> name = new Lazy<string>(delegate
                {
                    ThreadHelper.ThrowIfNotOnUIThread();

                    if (provider.GetService<IVsSolutionBuildManager>() is IVsSolutionBuildManager manager)
                    {
                        IVsProjectCfg[] cfgArray = new IVsProjectCfg[1];

                        if (manager.FindActiveProjectCfg(IntPtr.Zero, IntPtr.Zero, hierarchy, cfgArray) != 0 &&
                            cfgArray[0].get_DisplayName(out string value) != 0)
                        {
                            return value;
                        }
                    }
                    return string.Empty;
                });

                MatchEvaluator evaluator = delegate (Match m)
                {
                    ThreadHelper.ThrowIfNotOnUIThread();

                    Group group = m.Groups["name"];

                    string value = null;

                    bool success = !group.Success || (propertyStorage.GetPropertyValue(group.Value, name.Value, 1, out value) != 0);

                    return success ? m.Value : value;
                };

                path = vsMacroAndPropertyRegEx.Replace(path, evaluator);
            }

            return path;
        }
    }
}
