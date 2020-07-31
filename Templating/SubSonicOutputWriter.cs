using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Core.VisualStudio.Templating
{
    [Guid("8CCAA25E-1525-452B-82E6-2C0CFC16E277")]
    public class SubSonicOutputWriter
        : TextWriter
    {
        private const string title = "SUBSONIC";
        private readonly IServiceProvider provider;
        private Guid guid;

        private IVsOutputWindow _output;

        public SubSonicOutputWriter(IServiceProvider provider)
        {
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
            this.guid = GetType().GUID;
        }

        public TextWriter Initialize()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (provider.GetService(typeof(SVsOutputWindow)) is IVsOutputWindow output)
            {
                if (_output == null &&
                    output.CreatePane(ref guid, title, Convert.ToInt32(true), Convert.ToInt32(true)) != VSConstants.S_OK)
                {
                    return null;
                }

                _output = output;
            }

            return this;
        }

        public override Encoding Encoding => Encoding.UTF8;

        public override void Write(string value)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            IVsOutputWindowPane pane;

            if (_output.GetPane(ref guid, out pane) == VSConstants.S_OK)
            {
                pane.OutputString(value);
            }
        }

        public override void WriteLine()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            IVsOutputWindowPane pane;

            if (_output.GetPane(ref guid, out pane) == VSConstants.S_OK)
            {
                pane.OutputString(Environment.NewLine);
            }
        }

        public override void WriteLine(string value)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            IVsOutputWindowPane pane;

            if (_output.GetPane(ref guid, out pane) == VSConstants.S_OK)
            {
                pane.OutputString($"{value}{Environment.NewLine}");
            }
        }

        public override async System.Threading.Tasks.Task WriteAsync(string value)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            Write(value);
        }

        public override async System.Threading.Tasks.Task WriteLineAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            WriteLine();
        }

        public override async System.Threading.Tasks.Task WriteLineAsync(string value)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            WriteLine(value);
        }

        protected override void Dispose(bool disposing)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            _output?.DeletePane(ref guid);
            _output = null;

            base.Dispose(disposing);            
        }
    }
}
