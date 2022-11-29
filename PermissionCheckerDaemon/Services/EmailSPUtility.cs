using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Utilities;
using PermissionCheckerDaemon.Entities;
using PermissionCheckerDaemon.Interfaces;
using System.Collections.Generic;
using System.Configuration;
using static System.Console;
using System;
using System.Linq;

namespace PermissionCheckerDaemon.Services
{
    class EmailSPUtility : IEmail
    {
        private General _svcGeneral;

        public EmailSPUtility()
        {
            _svcGeneral = new General();
        }

        public void SendEmail(EmailObject email_object)
        {
            //using (ClientContext context = _svcGeneral.GetContext(ConfigurationManager.AppSettings["ConfigSiteUrl"]))
            using (ClientContext context = _svcGeneral.GetContextUsingCertificate(ConfigurationManager.AppSettings["ConfigSiteUrl"]))
            {
                try
                {
                    EmailProperties emailProperties = new EmailProperties();
                    //throw new Exception("Error from EmailSPUtility");
                    emailProperties.Subject = email_object.Content.Subject;
                    emailProperties.Body = email_object.Content.Body;

                    emailProperties.To = _svcGeneral.GetEmailAddressList(email_object.Recipients.TOAddress);
                    emailProperties.CC = _svcGeneral.GetEmailAddressList(email_object.Recipients.CCAddress);

                    emailProperties.AdditionalHeaders = GetEmailHeaders(email_object);

                    Utility.SendEmail(context, emailProperties);
                    context.ExecuteQueryRetry();

                    WriteLine("\nSent email");
                }
                catch (Exception ex)
                {
                    ErrorInfo.SecondaryErrorMessage += $"\nError occured while sending App Email.\nError message: {ex.Message}.\nStack trace: {ex.StackTrace}.";
                }
            }                
        }

        //private List<string> GetEmailAddressList(string emailAddress)
        //{
        //    var emailAddressesList = new List<string>();

        //    string[] splitEmails = emailAddress.Split(';');
        //    foreach (string email in splitEmails)
        //    {
        //        if (email != string.Empty)
        //        {
        //            emailAddressesList.Add(email);
        //        }
        //    }
            
        //    return emailAddressesList.Distinct().ToList();
        //}

        private Dictionary<string, string> GetEmailHeaders(EmailObject email_object)
        {
            if (email_object.Content.SetImportance)
            {
                return new Dictionary<string, string>()
                {
                    { "Content-Type", "text/plain" },
                    { "fAppendHtmlTag", "true" },
                    { "fHtmlEncode", "true" },
                    { "X-Priority", "1 (Highest)" },
                    { "X-MSMail-Priority", "High" },
                    { "Importance", "High" }
                };
            }

            return new Dictionary<string, string>()
            {
                { "Content-Type", "text/plain" },
                { "fAppendHtmlTag", "true" },
                { "fHtmlEncode", "true" }
            };
        }
    }
}