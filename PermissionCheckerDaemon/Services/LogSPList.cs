using Microsoft.SharePoint.Client;
using PermissionCheckerDaemon.Configuration;
using PermissionCheckerDaemon.Entities;
using PermissionCheckerDaemon.Interfaces;
using System.Collections.Generic;
using System.Configuration;
using static System.Console;
using System;

namespace PermissionCheckerDaemon.Services
{
    class LogSPList : IAppLogger
    {
        private General _svcGeneral;

        public LogSPList()
        {
            _svcGeneral = new General();
        }

        public void LogJob(List<AppDetails> apps)
        {
            //using (ClientContext context = _svcGeneral.GetContext(ConfigurationManager.AppSettings["ConfigSiteUrl"]))
            using (ClientContext context = _svcGeneral.GetContextUsingCertificate(ConfigurationManager.AppSettings["ConfigSiteUrl"]))
            {
                try
                {
                    List logList = context.Web.Lists.GetByTitle(Constants.AppLists.APPLICATION_LOG);
                    //throw new Exception("Error from LogSPList");
                    foreach (var app in apps)
                    {
                        foreach (var appList in app.AppLists)
                        {
                            ListItemCreationInformation listCreationInformation = new ListItemCreationInformation();
                            ListItem logItem = logList.AddItem(listCreationInformation);

                            logItem[Constants.LogColumns.APPLICATION_NAME] = app.ApplicationName;
                            logItem[Constants.LogColumns.LIST_NAME] = appList.ListName;
                            logItem[Constants.LogColumns.ID_CHECKED] = appList.AppTracer.IdChecked.Count > 0 ? string.Join(", ", appList.AppTracer.IdChecked) : string.Empty;

                            if (!appList.ListLevelInfo.HasError)
                                logItem[Constants.LogColumns.ERRORS_OCCURED] = appList.AppTracer.ErrorsOccurred.Count > 0 ? string.Join(", ", appList.AppTracer.ErrorsOccurred) : "No";
                            else
                                logItem[Constants.LogColumns.ERRORS_OCCURED] = $"Yes [{appList.ListLevelInfo.ErrorType}]";

                            logItem.Update();
                            context.ExecuteQueryRetry();
                        }
                    }

                    WriteLine("\nAplication log added");
                }
                catch (Exception ex)
                {
                    ErrorInfo.SecondaryErrorMessage += $"\nError occured while doing App Logging.\nError message: {ex.Message}.\nStack trace: {ex.StackTrace}.";
                }
            }                
        }
    }
}
