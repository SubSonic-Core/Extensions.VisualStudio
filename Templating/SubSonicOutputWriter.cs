using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;

namespace SubSonic.Core.VisualStudio.Templating
{
    [Guid("8CCAA25E-1525-452B-82E6-2C0CFC16E277")]
    public class SubSonicOutputWriter
        : TextWriter
    {
        private readonly IServiceProvider provider;
        private Guid guid;

        private IVsOutputWindow _output;

        public SubSonicOutputWriter(IServiceProvider provider)
        {
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
            this.guid = GetType().GUID;
        }

        public async Task InitializeAsync(string title, CancellationToken cancellationToken)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            if (provider.GetService(typeof(SVsOutputWindow)) is IVsOutputWindow output)
            {
                if (output.CreatePane(ref guid, title, Convert.ToInt32(true), Convert.ToInt32(true)) == VSConstants.S_OK)
                {
                    _output = output;
                }
            }
        }

        public TextWriter GetOutputTextWriter()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (_output.GetPane(ref guid, out IVsOutputWindowPane pane) == VSConstants.S_OK)
            {
                pane.Clear();

                return this;
            }

            return null;
        }

        public override Encoding Encoding => Encoding.UTF8;

        public override void Write(string value)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async () => await WriteAsync(value));
        }

        public override void WriteLine()
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async () => await WriteLineAsync());
        }

        public override void WriteLine(string value)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async () => await WriteLineAsync(value));
        }

        public override async Task WriteAsync(string value)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (_output.GetPane(ref guid, out IVsOutputWindowPane pane) == VSConstants.S_OK)
            {
                pane.OutputString(value);
            }
        }

        public override async Task WriteLineAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (_output.GetPane(ref guid, out IVsOutputWindowPane pane) == VSConstants.S_OK)
            {
                pane.OutputString(Environment.NewLine);
            }
        }

        public override async Task WriteLineAsync(string value)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (_output.GetPane(ref guid, out IVsOutputWindowPane pane) == VSConstants.S_OK)
            {
                pane.OutputString($"{value}{Environment.NewLine}");
            }
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
