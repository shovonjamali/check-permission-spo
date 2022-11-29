using Microsoft.SharePoint.Client;
using PermissionCheckerDaemon.Entities;
using PermissionCheckerDaemon.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using PermissionCheckerDaemon.Configuration;
using PermissionCheckerDaemon.Exceptions;

namespace PermissionCheckerDaemon.Services
{
    class AppClauseList : IAppClause
    {
        private Helper _helper;
        private General _svcGeneral;

        public AppClauseList()
        {
            _helper = new Helper();
            _svcGeneral = new General();
        }

        public IEnumerable<IGrouping<string, AppDetails>> CreateAppSecurityMaster()
        {
            //using (ClientContext context = _svcGeneral.GetContext(ConfigurationManager.AppSettings["ConfigSiteUrl"]))
            using (ClientContext context = _svcGeneral.GetContextUsingCertificate(ConfigurationManager.AppSettings["ConfigSiteUrl"]))
            {
                try
                {
                    var apps = GetAllApps(context);
                    //throw new Exception();
                    // Group applications by site
                    var grouped_by_site = apps.GroupBy(m => m.SiteURL);

                    return grouped_by_site;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        private List<AppDetails> GetAllApps(ClientContext context)
        {
            var apps = new List<AppDetails>();

            try
            {
                // Getting all the items from SST-ApplicationDetails list
                var items_apps = _svcGeneral.GetAllItems(context, listName: Constants.AppLists.APPLICATION_DETAILS, viewXml: new Helper().GetActiveItemQuery());                
                //throw new Exception();
                foreach (var item in items_apps)
                {
                    var app = new AppDetails(context);

                    string applicationName = Convert.ToString(item[Constants.AppDetailsColumns.APPLICATION_NAME]);
                    app.ApplicationName = applicationName;
                    app.SiteURL = Convert.ToString(item[Constants.AppDetailsColumns.SITE_URL]);
                    app.IsActive = Convert.ToBoolean(item[Constants.AppDetailsColumns.ACTIVE]);
                    app.ErrorEmailTo = Convert.ToString(item[Constants.AppDetailsColumns.ERROR_EMAIL_TO]);
                    app.ErrorEmailCC = Convert.ToString(item[Constants.AppDetailsColumns.ERROR_EMAIL_CC]);
                    ///TODO
                    //app.ClientID = Convert.ToString(item[Constants.AppDetailsColumns.CLIENT_ID]);
                    //app.ClientSecret = Convert.ToString(item[Constants.AppDetailsColumns.CLIENT_SECRET]);

                    app.AppLists = app[applicationName];

                    apps.Add(app);
                }

                return apps;
            }
            catch (ApplicationListDetailsException)
            {
                throw;
            }
            catch (ApplicationPermissionDetailsException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApplicationDetailsException(Constants.CustomExceptionMessages.APPLICATION_DETAILS, ex);
            }
        }       
    }
}