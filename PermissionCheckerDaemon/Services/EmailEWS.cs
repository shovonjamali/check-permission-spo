using Microsoft.Exchange.WebServices.Data;
using Microsoft.Identity.Client;
using PermissionCheckerDaemon.Entities;
using PermissionCheckerDaemon.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace PermissionCheckerDaemon.Services
{
    class EmailEWS : IEmail
    {
        private General _svcGeneral;
        
        public EmailEWS()
        {
            _svcGeneral = new General();
        }
        public void SendEmail(EmailObject email_object)
        {
            try
            {
                //ExchangeService service = new ExchangeService();
                //service.Url = new Uri(ConfigurationManager.AppSettings["ExchangeUrl"]);
                //service.Credentials = new WebCredentials(ConfigurationManager.AppSettings["ExchangeAccount"], ConfigurationManager.AppSettings["ExchangeAccountPass"]);

                Task<ExchangeService> ewsClients = GetExchangeServiceAsync();
                ExchangeService service = ewsClients.Result;

                EmailMessage message = new EmailMessage(service);
                message.Subject = email_object.Content.Subject;
                message.Body = email_object.Content.Body;
                foreach (var item in _svcGeneral.GetEmailAddressList(email_object.Recipients.TOAddress))
                {
                    message.ToRecipients.Add(item);
                }
                foreach(var item in _svcGeneral.GetEmailAddressList(email_object.Recipients.CCAddress))
                {
                    message.CcRecipients.Add(item);
                }
                message.Send();
                Console.WriteLine($"Email Sent {DateTime.Now}");

                if (email_object.FaultyEmail)
                {
                    message.ToRecipients.Clear();
                    message.CcRecipients.Clear();
                    message.ToRecipients.Add(ConfigurationManager.AppSettings["IT_SUPPORT_ADDRESS"]);
                    message.Subject = $"Source: Script Email {email_object.Content.Subject} : assigned to sharepoint-pool" ;
                    message.Send();
                    Console.WriteLine($"Email Sent for It support {DateTime.Now}");
                }
            }
            catch (Exception ex)
            {
                ErrorInfo.SecondaryErrorMessage += $"\nError occured while sending App Email.\nError message: {ex.Message}.\nStack trace: {ex.StackTrace}.";
            }
        }
        internal async Task<ExchangeService> GetExchangeServiceAsync()
        {
            try
            {
                // Using Microsoft.Identity.Client 4.46.0
                // Configure the MSAL client to get tokens
                PublicClientApplicationOptions pcaOptions = new PublicClientApplicationOptions
                {
                    ClientId = ConfigurationManager.AppSettings["EmailClientId"],
                    TenantId = ConfigurationManager.AppSettings["EmailTenantId"]
                };

                IPublicClientApplication pca = PublicClientApplicationBuilder.CreateWithApplicationOptions(pcaOptions).Build();

                // The permission scope required for EWS access
                string[] ewsScopes = new string[] { "https://outlook.office365.com/EWS.AccessAsUser.All" };

                // Make the interactive token request
                AuthenticationResult authResult = await pca.AcquireTokenByUsernamePassword(ewsScopes,
                            ConfigurationManager.AppSettings["ExchangeAccount"], ConfigurationManager.AppSettings["ExchangeAccountPass"])
                            .ExecuteAsync();

                // Configure the ExchangeService with the access token
                ExchangeService emailService = new ExchangeService();
                emailService.Url = new Uri("https://outlook.office365.com/EWS/Exchange.asmx");
                emailService.Credentials = new OAuthCredentials(authResult.AccessToken);

                return emailService;
            }
            catch (MsalException ex)
            {
                Console.WriteLine($"Error acquiring access token: {ex}");
                throw ex;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }
        }


        private SecureString GetSecureStringPassword(string password)
        {
            try
            {
                SecureString secureStringPassword = new SecureString();
                foreach (char c in password)
                {
                    secureStringPassword.AppendChar(c);
                }
                Console.WriteLine(" --> Constructed the secure password");
                return secureStringPassword;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Got error Reading the Password. Error: " + ex.Message);
                throw ex;
            }
        }

    }
}
