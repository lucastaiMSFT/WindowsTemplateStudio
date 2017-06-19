using EnvDTE;
using Microsoft.Templates.Core;
using Microsoft.Templates.Core.Diagnostics;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Templates.UI
{
    public class GenPostActions
    {
        public static string GetPostGenProjectID()
        {
            // Navigate through the DTE of the current VS instance to the created solution
            try
            {
                DTE dte = Package.GetGlobalService(typeof(SDTE)) as DTE;
                if (dte == null)
                {
                    AppHealth.Current.Error.TrackAsync("Could not get VS Desktop Env Service.").FireAndForget();
                    return null;
                }
                else
                {
                    // Query the DTE for Project GUID to uniquely identify the project created in the wizard for telem
                    IVsSolution solutionServ = Package.GetGlobalService(typeof(IVsSolution)) as IVsSolution;
                    if (solutionServ == null)
                    {
                        AppHealth.Current.Error.TrackAsync("Could not get Solution Manipulation Service.").FireAndForget();
                        return null;

                    }
                    // A single project should exist in the sln 
                    List<Project> projects = dte.Solution.OfType<Project>().ToList<Project>();
                    if (projects.Count() != 1)
                    {
                        AppHealth.Current.Error.TrackAsync("Generated solution contains more than one project.").FireAndForget();
                        return null;
                    }
                    Project p = projects.First<Project>();
                    String projUniqueName = p.UniqueName;
                    Guid projguid = new Guid();
                    IVsHierarchy projRoot = null;

                    solutionServ.GetProjectOfUniqueName(projUniqueName, out projRoot);
                    solutionServ.GetGuidOfProject(projRoot, out projguid);
                    AppHealth.Current.Info.TrackAsync($"GUID of project was {projguid.ToString()}.").FireAndForget();
                    return projguid.ToString();

                }

            }
            catch
            {
                return null;
            }
        }
    }
}

