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
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.TemplateEngine.Abstractions;
using Microsoft.TemplateEngine.Edge.Template;

namespace Microsoft.Templates.Core.Diagnostics
{
    public class TelemetryTracker : IDisposable
    {
        public const string PropertiesPrefix = "Wts";  
        
        public TelemetryTracker()
        {
        }

        public TelemetryTracker(Configuration config)
        { 
            TelemetryService.SetConfiguration(config);
        }

        public async Task TrackWizardCompletedAsync(WizardTypeEnum wizardType)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>()
            {
                { TelemetryProperties.WizardStatus, WizardStatusEnum.Completed.ToString() },
                { TelemetryProperties.WizardType, wizardType.ToString() },
                { TelemetryProperties.EventName, TelemetryEvents.Wizard}
            };

            await TelemetryService.Current.TrackEventAsync(TelemetryEvents.Wizard, properties).ConfigureAwait(false);
        }

        public async Task TrackWizardCompletedAsync()
        {
            Dictionary<string, string> properties = new Dictionary<string, string>()
            {
                { TelemetryProperties.WizardStatus, WizardStatusEnum.Completed.ToString() },
                { TelemetryProperties.WizardType, WizardTypeEnum.NewProject.ToString() },
                { TelemetryProperties.EventName, TelemetryEvents.Wizard}
            };

            await TelemetryService.Current.TrackEventAsync(TelemetryEvents.Wizard, properties).ConfigureAwait(false);
        }
        public async Task TrackWizardCancelledAsync(WizardTypeEnum wizardType)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>()
            {
                { TelemetryProperties.WizardStatus, WizardStatusEnum.Cancelled.ToString() },
                { TelemetryProperties.WizardType, wizardType.ToString() },
                { TelemetryProperties.EventName, TelemetryEvents.Wizard}
            };

            await TelemetryService.Current.TrackEventAsync(TelemetryEvents.Wizard, properties).ConfigureAwait(false);
        }

        public async Task TrackProjectGenAsync(ITemplateInfo template, string appFx, TemplateCreationResult result, string projectID, int? pagesCount = null, int? featuresCount = null, double? timeSpent = null)
        {
            if (template == null)
                throw new ArgumentNullException("template");

            if (result == null)
                throw new ArgumentNullException("result");

            if (projectID == null)
                throw new ArgumentNullException("projectID");

            if (template.GetTemplateType() != TemplateType.Project)
            {
                return;
            }

            GenStatusEnum telemetryStatus = result.Status == CreationResultStatus.Success ? GenStatusEnum.Completed : GenStatusEnum.Error;

            await TrackProjectAsync(telemetryStatus, template.Name, template.GetProjectType(), appFx, projectID, pagesCount, featuresCount, timeSpent, result.Status, result.Message);
        }

        public async Task TrackItemGenAsync(ITemplateInfo template, string appFx, TemplateCreationResult result, string projectID)
        {
            if (template == null)
                throw new ArgumentNullException("template");

            if (result == null)
                throw new ArgumentNullException("result");

            if (projectID == null)
                throw new ArgumentNullException("projectID");

            if (template != null && result != null)
            {
                switch (template.GetTemplateType())
                {
                    case TemplateType.Page:
                        await TrackItemGenAsync(TelemetryEvents.PageGen, template, appFx, result, projectID);
                        break;
                    case TemplateType.Feature:
                        await TrackItemGenAsync(TelemetryEvents.FeatureGen, template, appFx, result, projectID);
                        break;
                    case TemplateType.Unspecified:
                        break;
                }
            }
        }

        private async Task TrackProjectAsync(GenStatusEnum status, string templateName, string appType, string appFx, string projectID, int? pagesCount = null, int? featuresCount = null, double? timeSpent = null, CreationResultStatus genStatus = CreationResultStatus.Success, string message = "")
        {
            Dictionary<string, string> properties = new Dictionary<string, string>()
            {
                { TelemetryProperties.Status, status.ToString() },
                { TelemetryProperties.ProjectID, projectID },
                { TelemetryProperties.ProjectType, appType },
                { TelemetryProperties.Framework, appFx },
                { TelemetryProperties.TemplateName, templateName },
                { TelemetryProperties.GenEngineStatus, genStatus.ToString() },
                { TelemetryProperties.GenEngineMessage, message },
                { TelemetryProperties.EventName, TelemetryEvents.ProjectGen}
            };

            Dictionary<string, double> metrics = new Dictionary<string, double>();

            if (pagesCount.HasValue)
            {
                metrics.Add(TelemetryMetrics.PagesCount, pagesCount.Value);
            }

            if (timeSpent.HasValue)
            {
                metrics.Add(TelemetryMetrics.TimeSpent, timeSpent.Value);
            }

            if (featuresCount.HasValue)
            {
                metrics.Add(TelemetryMetrics.FeaturesCount, featuresCount.Value);
            }

            await TelemetryService.Current.TrackEventAsync(TelemetryEvents.ProjectGen, properties, metrics).ConfigureAwait(false);
        }

        private async Task TrackItemGenAsync(string eventToTrack, ITemplateInfo template, string appFx, TemplateCreationResult result, string projectID)
        {
            GenStatusEnum telemetryStatus = result.Status == CreationResultStatus.Success ? GenStatusEnum.Completed : GenStatusEnum.Error;

            await TrackItemGenAsync(eventToTrack, telemetryStatus, template.GetProjectType(), appFx, template.Name, projectID, result.Status, result.Message);
        }

        private async Task TrackItemGenAsync(string eventToTrack,  GenStatusEnum status, string appType, string pageFx, string templateName, string projectID, CreationResultStatus genStatus= CreationResultStatus.Success, string message="")
        {
            Dictionary<string, string> properties = new Dictionary<string, string>()
            {
                { TelemetryProperties.Status, status.ToString() },
                { TelemetryProperties.Framework, pageFx },
                { TelemetryProperties.TemplateName, templateName },
                { TelemetryProperties.GenEngineStatus, genStatus.ToString() },
                { TelemetryProperties.GenEngineMessage, message },
                { TelemetryProperties.EventName, eventToTrack},
                { TelemetryProperties.ProjectID, projectID }
            };

            await TelemetryService.Current.TrackEventAsync(eventToTrack, properties).ConfigureAwait(false);
        }

        ~TelemetryTracker()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources 
                TelemetryService.Current.Dispose();
            }
            //free native resources if any.
        }
    }
}
