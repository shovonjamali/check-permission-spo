using PermissionCheckerDaemon.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Text;
using PermissionCheckerDaemon.Configuration;
using Microsoft.SharePoint.Client;

namespace PermissionCheckerDaemon.Services
{
    class EmailManager
    {
        private Helper _helper = null;
        private General _svcGeneral;

        public EmailManager()
        {
            _helper = new Helper();
            _svcGeneral = new General();
        }

        internal EmailObject GetEmailObject(List<AppDetails> checked_apps)
        {
            try
            {
                bool hasError = _helper.HasFaultyItem(checked_apps);
                //throw new Exception();
                return new EmailObject(GetEmailRecipients(hasError, checked_apps), GetEmailContent(hasError, checked_apps), hasError);
            }
            catch (Exception ex)
            {
                throw new Exception(Constants.CustomExceptionMessages.EMAIL_OBJECT, ex);
            }
        }

        private EmailAddress GetEmailRecipients(bool hasError, List<AppDetails> checked_apps)
        {
            var list_to_addresses = checked_apps.Select(a => a.ErrorEmailTo);
            //var to_addresses = hasError == false ? string.Join(";", list_to_addresses) : string.Join(";", list_to_addresses) + ";" + ConfigurationManager.AppSettings["IT_SUPPORT_ADDRESS"];
            var to_addresses = string.Join(";", list_to_addresses);
            // Ensuring 'To' recipients to make sure they are getting the email
            EnsureRecipients(to_addresses);

            string cc_addresses = string.Empty;
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["CCinEmail"]))
            {
                cc_addresses = string.Join(";", checked_apps.Select(a => a.ErrorEmailCC));
                // Ensuring 'CC' recipients to make sure they are getting the email
                EnsureRecipients(cc_addresses);
            }

            return new EmailAddress(to_addresses, cc_addresses);
        }

        private void EnsureRecipients(string users)
        {
            string current_user = string.Empty;

            //using (ClientContext context = _svcGeneral.GetContext(ConfigurationManager.AppSettings["ConfigSiteUrl"]))
            using (ClientContext context = _svcGeneral.GetContextUsingCertificate(ConfigurationManager.AppSettings["ConfigSiteUrl"]))
            {
                try
                {
                    var users_list = users.Split(';').Distinct().ToArray();

                    foreach (var user in users_list)
                    {
                        current_user = user;
                        context.Web.EnsureUser(current_user);
                        context.ExecuteQuery();
                    }

                    context.ExecuteQueryRetry();
                }
                catch (Exception ex)
                {
                    ErrorInfo.SecondaryErrorMessage += $"\nError occured while ensuring Recipient - {current_user}.\nError message: {ex.Message}.\nStack trace: {ex.StackTrace}.";
                }
            }
        }

        private EmailContent GetEmailContent(bool hasError, List<AppDetails> checked_apps)
        {
            var emailContent = new EmailContent();

            if (hasError)
            {
                emailContent.SetImportance = true;
                emailContent.Subject = $"SharePoint Security Tool Job Ran at {DateTime.Now.ToShortDateString()} With Errors";
                emailContent.Body = GetFaultyItemsBody(checked_apps);           
            }
            else
            {
                emailContent.SetImportance = false;
                emailContent.Subject = $"SharePoint Security Tool Job Ran at {DateTime.Now.ToShortDateString()} Successfully";
                emailContent.Body = GetSuccessItemsBody();
            }

            return emailContent;
        }

        private string GetSuccessItemsBody()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Hi Admins, <br/><br/>");
            sb.Append($"Job for the Security Tool Ran at {DateTime.Now.ToShortDateString()} Successfully.<br/><br/>");
            sb.Append(GetEmailFooter());
            return sb.ToString();
        }

        private string GetFaultyItemsBody(List<AppDetails> checked_apps)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Hi Admins," + "<p style=\"color: #FF0033\">Following are the records monitored by SharePoint Security Tool. Please review and take appropriate actions:</p>");
            sb.Append("<table style=\"border-collapse: collapse; font-family: arial, sans-serif; width: 100%;\">");
            sb.Append("<tr style=\"background-color: #6FA1D2; color: #ffffff;\">");

            sb.Append("<th style=\"border: 1px solid #5c87b2; padding: 5px;\">Sl.</th>");
            sb.Append("<th style=\"border: 1px solid #5c87b2; padding: 5px;\">Site URL</th>");
            sb.Append("<th style=\"border: 1px solid #5c87b2; padding: 5px;\">Application Name</th>");
            sb.Append("<th style=\"border: 1px solid #5c87b2; padding: 5px;\">Site/List Name</th>");
            sb.Append("<th style=\"border: 1px solid #5c87b2; padding: 5px;\">ID</th>");
            sb.Append("<th style=\"border: 1px solid #5c87b2; padding: 5px;\">Error Type</th>");

            sb.Append("</tr>");

            int index = 1;
            foreach (var app in checked_apps)
            {
                foreach (var list in app.AppLists)
                {
                    if(list.SecurityCheck.PermissionLevel == Constants.AppPermissionDetailsColumns.PermissionLevelValues.ITEM_LEVEL)
                    {
                        foreach (var processed_item in list.AppTracer.InspectingItems)
                        {
                            if (!string.IsNullOrEmpty(processed_item.ErrorType))
                            {
                                sb.Append(GetTableRow(app, list, index, requestItem: processed_item));
                                index++;
                            }
                        }
                    }
                    else
                    {
                        if(list.ListLevelInfo.HasError)
                        {
                            sb.Append(GetTableRow(app, list, index));
                            index++;
                        }                        
                    }
                }
            }

            sb.Append("</table><br/>");
            sb.Append(GetEmailFooter());

            return sb.ToString();
        }

        private string GetTableRow(AppDetails app, AppListDetails list, int index, RequestItem requestItem = null)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<tr style=\"color:#555555;\">");

            sb.Append("<td style=\"border: 1px solid #5c87b2; padding: 5px; text-align: center;\">" + index + "</td>");
            sb.Append("<td style=\"border: 1px solid #5c87b2; padding: 5px;\"><a href=\"" + app.SiteURL + "\" target=\"_blank\">" + app.SiteURL + "</td>");
            sb.Append("<td style=\"border: 1px solid #5c87b2; padding: 5px;\">" + app.ApplicationName + "</td>");
            sb.Append("<td style=\"border: 1px solid #5c87b2; padding: 5px;\">" + list.ListName + "</td>");

            if (!list.ListLevelInfo.HasError)
                sb.Append("<td style=\"border: 1px solid #5c87b2; padding: 5px;\"><a href=\"" + requestItem?.PermissionUrl + "\" target=\"_blank\" title=\"" + requestItem?.Title + "\">" + requestItem?.ItemId + "</a></td>");
            else
                sb.Append("<td style=\"border: 1px solid #5c87b2; padding: 5px;\"><a href=\"" + list.ListLevelInfo.PermissionUrl + "\" target=\"_blank\">" + "N/A" + "</a></td>");

            string errorType = list.ListLevelInfo.HasError != true ? requestItem?.ErrorType : list.ListLevelInfo.ErrorType;
            sb.Append("<td style=\"border: 1px solid #5c87b2; padding: 5px;\">" + errorType + "</td>");

            sb.Append("</tr>");

            return Convert.ToString(sb);
        }

        private string GetEmailFooter()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Best Regards, <br/>abc SPO Security Services.");
            sb.Append("<br><br><hr>");
            sb.Append("<span style=\"font-size: 13px; font-weight: normal\">[THIS IS AN AUTOMATICALLY GENERATED EMAIL - PLEASE DO NOT REPLY DIRECTLY TO THIS EMAIL AS YOUR MESSAGE WILL NOT BE RECEIVED AND WILL BE RETURNED TO YOU BY THE MAIL SERVER]</span>");
            return sb.ToString();
        }
    }
}