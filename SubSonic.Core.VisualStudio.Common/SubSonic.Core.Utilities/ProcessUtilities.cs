using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SubSonic.Core.Utilities
{
    public static class ProcessUtilities
    { 
        public static Task<int> StartProcess(ProcessStartInfo psi, TextWriter stdOut, TextWriter stdError, out Process process, CancellationToken cancellationToken)
        {
            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();
            
            if (cancellationToken.CanBeCanceled && cancellationToken.IsCancellationRequested)
            {
                tcs.TrySetCanceled();

                process = null;

                return tcs.Task;
            }

            psi.RedirectStandardOutput |= stdOut != null;
            psi.RedirectStandardError |= stdError != null;

            try
            {
                Process p = Process.Start(psi);

                if (cancellationToken.CanBeCanceled)
                {
                    cancellationToken.Register(() =>
                    {
                        try
                        {
                            if (!p.HasExited)
                            {
                                p.Kill();
                            }
                        }
                        catch (InvalidOperationException ex)
                        {
                            if (ex.Message.IndexOf("already exited", StringComparison.Ordinal) < 0)
                            {
                                throw;
                            }
                        }
                    });
                }

                bool
                    outputDone = false,
                    errorDone = false,
                    exitDone = false;

                p.EnableRaisingEvents = true;

                if (psi.RedirectStandardError)
                {
                    bool stdErrorInitialized = false;

                    p.ErrorDataReceived += (sender, e) =>
                    {
                        try
                        {
                            if (e.Data == null)
                            {
                                errorDone = true;
                                if (exitDone && outputDone)
                                {
                                    tcs.TrySetResult(p.ExitCode);
                                }
                                return;
                            }

                            if (stdErrorInitialized)
                            {
                                stdError.WriteLine();
                            }
                            stdError.Write(e.Data);
                            stdErrorInitialized = true;
                        }
                        catch (Exception ex)
                        {
                            tcs.TrySetException(ex);
                        }
                    };
                    p.BeginErrorReadLine();
                }
                else
                {
                    errorDone = true;
                }

                if (psi.RedirectStandardOutput)
                {
                    bool stdOutInitialized = false;
                    p.OutputDataReceived += (sender, e) =>
                    {
                        try
                        {
                            if (e.Data == null)
                            {
                                outputDone = true;
                                if (exitDone && errorDone)
                                {
                                    tcs.TrySetResult(p.ExitCode);
                                }
                                return;
                            }

                            if (stdOutInitialized)
                            {
                                stdOut.WriteLine();
                            }
                            stdOut.Write(e.Data);
                            stdOutInitialized = true;
                        }
                        catch (Exception ex)
                        {
                            tcs.TrySetException(ex);
                        }
                    };
                    p.BeginOutputReadLine();
                }
                else
                {
                    outputDone = true;
                }

                p.Exited += (sender, e) =>
                {
                    exitDone = true;
                    if (errorDone && outputDone)
                    {
                        tcs.TrySetResult(p.ExitCode);
                    }
                };

                process = p;

                return tcs.Task;
            }
            catch (Exception ex)
            {
                stdError.Write(ex.ToString());

                tcs.TrySetException(ex);

                process = null;

                return tcs.Task;
            }
        }
    }
}
