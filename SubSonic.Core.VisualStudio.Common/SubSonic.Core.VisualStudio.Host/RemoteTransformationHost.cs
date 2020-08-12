﻿using Mono.TextTemplating;
using Mono.TextTemplating.CodeCompilation;
using Mono.VisualStudio.TextTemplating;
using Mono.VisualStudio.TextTemplating.VSHost;
using SubSonic.Core.VisualStudio.Common;
using SubSonic.Core.VisualStudio.Host;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Core.VisualStudio.Services
{
    [Serializable]
    public class RemoteTransformationHost
        : ProcessEngineHost
    {
        private static readonly Dictionary<string, string> KnownAssemblyNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "System", Path.GetFileName(typeof(System.Uri).Assembly.Location) },
            { "System.Core", Path.GetFileName(typeof(System.Linq.Enumerable).Assembly.Location) },
            { "System.Data", Path.GetFileName(typeof(System.Data.DataTable).Assembly.Location) },
            { "System.Linq", Path.GetFileName(typeof(System.Linq.Enumerable).Assembly.Location) },
            { "System.Xml", Path.GetFileName(typeof(System.Xml.XmlAttribute).Assembly.Location) }
        };

        private readonly List<string> includePaths;
        private readonly Dictionary<RuntimeKind, List<string>> referencePaths;
        private readonly Dictionary<ParameterKey, string> parameters;
        private readonly Dictionary<string, KeyValuePair<string, string>> directiveProcessors;
        
        private readonly TemplateErrorCollection errors;

        [NonSerialized]
        private GetHostOptionEventHandler getHostOptionEventHandler;

        public event GetHostOptionEventHandler GetHostOptionEventHandler
        {
            add
            {
                getHostOptionEventHandler += value;
            }
            remove
            {
                getHostOptionEventHandler -= value;
            }
        }

        [NonSerialized]
        private ResolveAssemblyReferenceEventHandler resolveAssemblyReferenceEventHandler;

        public event ResolveAssemblyReferenceEventHandler ResolveAssemblyReferenceEventHandler
        {
            add
            {
                resolveAssemblyReferenceEventHandler += value;
            }
            remove
            {
                resolveAssemblyReferenceEventHandler -= value;
            }
        }

        [NonSerialized]
        private ExpandAllVariablesEventHandler expandAllVariablesEventHandler;

        public event ExpandAllVariablesEventHandler ExpandAllVariablesEventHandler
        {
            add
            {
                expandAllVariablesEventHandler += value;
            }
            remove
            {
                expandAllVariablesEventHandler -= value;
            }
        }

        public RemoteTransformationHost()
            : base()
        {
            parameters = new Dictionary<ParameterKey, string>();
            includePaths = new List<string>();
            referencePaths = new Dictionary<RuntimeKind, List<string>>();
            directiveProcessors = new Dictionary<string, KeyValuePair<string, string>>();
            errors = new TemplateErrorCollection();
        }

        public Dictionary<RuntimeKind, List<string>> ReferencePaths => referencePaths;

        public IList<string> RuntimeReferencePaths
        {
            get
            {
                RuntimeKind runtime = (RuntimeKind)(OnGetHostOption(nameof(TemplateSettings.RuntimeKind)) ?? RuntimeKind.NetCore);

                if (referencePaths.ContainsKey(runtime))
                {
                    return referencePaths[runtime];
                }

                return new List<string>();
            }
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task InitializeAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Initialize();
        }

        protected override void Initialize()
        {
            foreach (RuntimeInfo info in RuntimeInfo.GetAllValidRuntimes())
            {
                AddReferencePath(info.Kind, info.RuntimeDirectory);
            }

            base.Initialize();

            if (!StandardAssemblyReferences.Any(x => x == GetType().Assembly.Location))
            {
                StandardAssemblyReferences.Add(GetType().Assembly.Location);
            }
        }

        /// <summary>
        /// add a reference path for a particular runtime
        /// </summary>
        /// <param name="runtime"></param>
        /// <param name="directoryPath"></param>
        public void AddReferencePath(RuntimeKind runtime, string directoryPath)
        {
            if (!ReferencePaths.ContainsKey(runtime))
            {
                ReferencePaths[runtime] = new List<string>();
            }

            if (!ReferencePaths[runtime].Any(x => x.Equals(directoryPath, StringComparison.OrdinalIgnoreCase)))
            {
                ReferencePaths[runtime].Add(directoryPath);
            }
        }

        public override ITextTemplatingSession CreateSession()
        {
            ITextTemplatingSession session = new TextTemplatingSession();

            session[nameof(TemplateFile)] = TemplateFile;

            return session;
        }

        private object OnGetHostOption(string optionName)
        {
            if (getHostOptionEventHandler != null)
            {
                return getHostOptionEventHandler(this, new GetHostOptionEventArgs(optionName));
            }
            return default;
        }

        public override object GetHostOption(string optionName)
        {
            return OnGetHostOption(optionName);
        }

        public void AddDirective(string key, KeyValuePair<string, string> processor)
        {
            directiveProcessors.Add(key, processor);
        }

        public void AddParameter(ParameterKey key, string value)
        {
            parameters.Add(key, value);
        }

        public override bool LoadIncludeText(string requestFileName, out string content, out string location)
        {
            bool success = false;

            content = "";
            location = ResolvePath(requestFileName);

            if (location == null || !File.Exists(location))
            {
                foreach (string path in includePaths)
                {
                    string f = Path.Combine(path, requestFileName);

                    if (File.Exists(f))
                    {
                        location = f;
                        break;
                    }
                }
            }

            if (location.IsNullOrEmpty())
            {
                return success;
            }

            try
            {
                content = File.ReadAllText(location);

                success = true;
            }
            catch (IOException ex)
            {
                _ = LogErrorAsync($"Could not read included file '{location}':\n{ex}");
            }

            return success;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task LogErrorsAsync(TemplateErrorCollection errors)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            LogErrors(errors);
        }



#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task LogErrorAsync(string message, Location location, bool isWarning = default)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            LogError(message, location, isWarning);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task LogErrorAsync(string message, bool isWarning = default)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            LogError(message, isWarning);
        }

#if NETFRAMEWORK
        public override AppDomain ProvideTemplatingAppDomain(string content)
        {
            return null;    // not supported in a framework to netcore solution
        }
#endif

        public override string ResolveAssemblyReference(string assemblyReference)
        {
            return OnResolveAssemblyReference(assemblyReference);
        }

        private string OnResolveAssemblyReference(string assemblyReference)
        {
            if (!Path.IsPathRooted(assemblyReference))
            {
                KnownAssemblyNames.TryGetValue(assemblyReference, out string fileName);

                foreach (string referencePath in RuntimeReferencePaths)
                {
                    string xpath = Path.Combine(referencePath, fileName ?? assemblyReference);
                    if (File.Exists(xpath))
                    {
                        return xpath;
                    }
                    else if (File.Exists($"{xpath}.dll"))
                    {
                        return $"{xpath}.dll";
                    }
                }

                if (resolveAssemblyReferenceEventHandler != null)
                {
                    return resolveAssemblyReferenceEventHandler(this, new ResolveAssemblyReferenceEventArgs(assemblyReference));
                }
            }

            return assemblyReference;
        }

        private string OnExpandAllVariables(string filePath)
        {
            if (expandAllVariablesEventHandler != null)
            {
                return expandAllVariablesEventHandler(this, new ExpandAllVariablesEventArgs(filePath));
            }
            return filePath;
        }

        public override string ResolvePath(string path)
        {
            if (path.IsNotNullOrEmpty())
            {
                path = OnExpandAllVariables(path);

                if (Path.IsPathRooted(path))
                {
                    return path;
                }
            }

            string directory;
            if (TemplateFile.IsNullOrEmpty())
            {
                directory = Environment.CurrentDirectory;
            }
            else
            {
                directory = Path.GetDirectoryName(Path.GetFullPath(TemplateFile));
            }

            if (path.IsNullOrEmpty())
            {
                return directory;
            }

            string test = Path.Combine(directory, path);
            if (File.Exists(test) || Directory.Exists(test))
            {
                return test;
            }

            return path;
        }        

        public override Type ResolveDirectiveProcessor(string processorName)
        {
            if (!directiveProcessors.TryGetValue(processorName, out KeyValuePair<string, string> value))
            {
                throw new Exception(string.Format("No directive processor registered as '{0}'", processorName));
            }

            var asmPath = ResolveAssemblyReference(value.Value);
            if (asmPath.IsNullOrEmpty())
            {
                throw new Exception(string.Format("Could not resolve assembly '{0}' for directive processor '{1}'", value.Value, processorName));
            }
            var asm = Assembly.LoadFrom(asmPath);
            return asm.GetType(value.Key, true);
        }

        public override string ResolveParameterValue(string directiveId, string processorName, string parameterName)
        {
            var key = new ParameterKey(processorName, directiveId, parameterName);
            if (parameters.TryGetValue(key, out var value))
            {
                return value;
            }
            if (processorName != null || directiveId != null)
            {
                return ResolveParameterValue(null, null, parameterName);
            }
            return null;
        }
    }
}
