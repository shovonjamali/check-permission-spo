using Microsoft.SharePoint.Client;
using OfficeDevPnP.Core;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using static System.Console;

namespace PermissionCheckerDaemon.Services
{
    class AuthenticationAssistant
    {
        private AuthenticationManager _authManager = null;

        private readonly string _debugCertificatePath = string.Empty;
        private readonly string _debugCertificatePassword = string.Empty;

        private readonly string _azureAppClientId = string.Empty;
        private readonly string _tenant = string.Empty;

        private readonly string _thumbPrint = string.Empty;

        public AuthenticationAssistant()
        {
            _authManager = new AuthenticationManager();

            _debugCertificatePath = "";
            _debugCertificatePassword = "";

            _azureAppClientId = ConfigurationManager.AppSettings["AADAppClientID"];
            _tenant = ConfigurationManager.AppSettings["Tenant"];

            _thumbPrint = ConfigurationManager.AppSettings["CertificateThumbPrint"];
        }

        internal ClientContext GetContextUsingUserCredential(string siteUrl) =>
            _authManager.GetSharePointOnlineAuthenticatedContextTenant(siteUrl, ConfigurationManager.AppSettings["UserUPN"], ConfigurationManager.AppSettings["UserPassword"]);

        internal ClientContext GetContextUsingACS(string siteUrl) => 
            _authManager.GetAppOnlyAuthenticatedContext(siteUrl, ConfigurationManager.AppSettings["ClientID"], ConfigurationManager.AppSettings["ClientSecret"]);

        internal ClientContext GetContextUsingACS(string siteUrl, string clientID, string clientSecret) => 
            _authManager.GetAppOnlyAuthenticatedContext(siteUrl, clientID, clientSecret);        

        internal ClientContext GetContextUsingAzureADAppUsingCertificateFromStore(string siteUrl)
        {
            return _authManager.GetAzureADAppOnlyAuthenticatedContext(siteUrl, _azureAppClientId, _tenant, StoreName.My, StoreLocation.CurrentUser, _thumbPrint);
        }

        internal ClientContext GetContextUsingAzureADAppUsingCertificateFromDirectory(string siteUrl, string path, string password)
        {
            return _authManager.GetAzureADAppOnlyAuthenticatedContext(siteUrl, _azureAppClientId, _tenant, path, password);
        }

        private X509Certificate2 GetCertificate()
        {
            var certificate = Debugger.IsAttached ? GetCertificateFromDirectory(_debugCertificatePath, _debugCertificatePassword) : GetCertificateFromStore(ConfigurationManager.AppSettings["CertificateThumbprint"]);
            //return new ClientAssertionCertificate(clientId, certificate);
            return certificate;
        }

        private X509Certificate2 GetCertificateFromDirectory(string path, string password)
        {
            return new X509Certificate2(System.IO.Path.GetFullPath(path), password, X509KeyStorageFlags.MachineKeySet);
        }

        private X509Certificate2 GetCertificateFromStore(string thumbPrint)
        {
            bool validOnly = false;

            if (string.IsNullOrEmpty(thumbPrint))
                throw new ArgumentNullException("thumbPrint", "Argument 'thumbPrint' cannot be 'null' or 'string.empty'");

            using (X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
                store.Open(OpenFlags.ReadOnly);

                X509Certificate2Collection certificates = store.Certificates.Find(
                                            X509FindType.FindByThumbprint,
                                            thumbPrint,
                                            validOnly);

                // Get the first cert with the thumbprint
                X509Certificate2 certificate = certificates.OfType<X509Certificate2>().FirstOrDefault();

                if (certificate is null)
                    throw new Exception($"Certificate with thumbprint {thumbPrint} was not found");

                // Use certificate
                WriteLine(certificate.FriendlyName);

                // Consider to call Dispose() on the certificate after it's being used, avaliable in .NET 4.6 and later
                return certificate;
            }
        }
    }
}
