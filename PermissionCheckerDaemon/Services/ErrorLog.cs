using Microsoft.SharePoint.Client;
using PermissionCheckerDaemon.Entities;
using System;
using System.Configuration;
using PermissionCheckerDaemon.Configuration;
using static System.Console;

namespace PermissionCheckerDaemon.Services
{
    class ErrorLog
    {
        private General _svcGeneral;

        public ErrorLog()
        {
            _svcGeneral = new General();
        }

        internal void WriteLog()
        {
            string error_message = GetAggregatedMessage();

            if(!string.IsNullOrEmpty(error_message))
            {
                //using (ClientContext context = _svcGeneral.GetContext(ConfigurationManager.AppSettings["ConfigSiteUrl"]))
                using (ClientContext context = _svcGeneral.GetContextUsingCertificate(ConfigurationManager.AppSettings["ConfigSiteUrl"]))
                {
                    List list = context.Web.Lists.GetByTitle(Constants.AppLists.APPLICATION_ERROR_LOG);
                    ListItemCreationInformation listCreationInformation = new ListItemCreationInformation();
                    ListItem item = list.AddItem(listCreationInformation);

                    item[Constants.AppErrorLogColumns.TITLE] = $"Error# {Guid.NewGuid()}";
                    item[Constants.AppErrorLogColumns.MESSAGE] = error_message;

                    item.Update();
                    context.ExecuteQueryRetry();

                    WriteLine("\nAplication error log added");
                }
            }            
        }

        private string GetAggregatedMessage()
        {
            string aggregated_message = string.Empty;

            if (!string.IsNullOrEmpty(ErrorInfo.SecondaryErrorMessage))
            {
                aggregated_message = $"Internal Errors\n";
                aggregated_message += "================";
                aggregated_message += ErrorInfo.SecondaryErrorMessage;
                aggregated_message += "\n\n";
            }

            if(!string.IsNullOrEmpty(ErrorInfo.PrimaryErrorMessage))
            {
                aggregated_message += "Exception\n";
                aggregated_message += "================\n";
                aggregated_message += ErrorInfo.PrimaryErrorMessage;
            }

            return aggregated_message;
        }
    }
}