﻿// ******************************************************************
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************

using System;
using System.Diagnostics;
using SystemTasks = System.Threading.Tasks;

using Microsoft.Templates.Core.Gen;

namespace Microsoft.Templates.Core.Diagnostics
{
    public class ShellHealthWriter : IHealthWriter
    {
        public async SystemTasks.Task WriteExceptionAsync(Exception ex, string message = null)
        {
            if (GenContext.ToolBox.Shell != null)
            {
                await SafeTrackAsync(() =>
                {
                    string header = $"========== Tracked Exception [{DateTime.Now.ToString("yyyyMMdd hh:mm:ss.fff")}] ==========\n";
                    GenContext.ToolBox.Shell.WriteOutput(header);

                    if (message != null)
                    {
                        GenContext.ToolBox.Shell.WriteOutput($"AdditionalMessage: {message}\n");
                    }

                    GenContext.ToolBox.Shell.WriteOutput($"{ex.ToString()}\n");

                    string footer = $"{new string('-', header.Length - 2)}\n";
                    GenContext.ToolBox.Shell.WriteOutput(footer);
                });
            }
        }

        public async SystemTasks.Task WriteTraceAsync(TraceEventType eventType, string message, Exception ex = null)
        {
            if (GenContext.ToolBox.Shell != null)
            {
                await SafeTrackAsync(() =>
                {
                    string eventMessage = $"[{DateTime.Now.ToString("hh:mm:ss.fff")} - {eventType.ToString()}]::{message}\n";
                    GenContext.ToolBox.Shell.WriteOutput(eventMessage);

                    if (ex != null)
                    {
                        string header = $"----------- Addtional Exception Info -----------\n";
                        string footer = $"{new string('-', header.Length - 2)}\n";
                        string exceptionInfo = header + $"{ex.ToString()}\n" + footer;
                        GenContext.ToolBox.Shell.WriteOutput(exceptionInfo);
                    }
                });
            }
        }

        private async SystemTasks.Task SafeTrackAsync(Action trackAction)
        {
            try
            {
                var task = SystemTasks.Task.Run(() =>
                {
                    trackAction();
                });

                await task;
            }
            catch (AggregateException aggEx)
            {
                foreach (Exception ex in aggEx.InnerExceptions)
                {
                    Trace.TraceError("Error writing to Visual Studio Output: {0}", ex.ToString());
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error writing to Visual Studio Output: {0}", ex.ToString());
            }
        }

        public bool AllowMultipleInstances()
        {
            return false;
        }
    }
}
