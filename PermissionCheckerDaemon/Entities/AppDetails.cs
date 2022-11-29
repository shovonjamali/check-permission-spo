using Microsoft.SharePoint.Client;
using PermissionCheckerDaemon.Services;
using System.Collections.Generic;
using PermissionCheckerDaemon.Configuration;
using System;
using PermissionCheckerDaemon.Exceptions;
using System.Linq;

namespace PermissionCheckerDaemon.Entities
{
    class AppDetails
    {
        public string ApplicationName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string SiteURL { get; set; }
        public string ErrorEmailTo { get; set; }
        public string ErrorEmailCC { get; set; }
        public string ClientID { get; set; }
        public string ClientSecret { get; set; }
        
        public List<AppListDetails> AppLists { get; set; } = new List<AppListDetails>();

        public Dictionary<string, int> PermissionOrder = new Dictionary<string, int>();

        private General _svcGeneral;
        private List<AppListDetails> _appListDetails;

        public AppDetails() { }

        public AppDetails(ClientContext context)
        {
            _svcGeneral = new General();

            _appListDetails = GetAllLists(context);

            PermissionOrder = new PermissionOrder(context).DicPermissionOrder;
        }

        public List<AppListDetails> this[string applicationName]
        {
            get
            {
                // Returning the lists associated with this application based on the name
                return _appListDetails.Where(l => l.ApplicationName == applicationName).ToList();
            }
        }

        private List<AppListDetails> GetAllLists(ClientContext context)
        {
            var appListDetails = new List<AppListDetails>();

            try
            {
                // Getting all the items from SST-ApplicationListDetails list
                var items_app_lists = _svcGeneral.GetAllItems(context, listName: Constants.AppLists.APPLICATION_LIST_DETAILS, viewXml: new Helper().GetActiveItemQuery());
                
                foreach (var item in items_app_lists)
                {
                    var appList = new AppListDetails(context);

                    appList.ApplicationName = ((FieldLookupValue)item[Constants.AppListDetailsColumns.APPLICATION_NAME]).LookupValue;
                    appList.ListName = Convert.ToString(item[Constants.AppListDetailsColumns.LIST_NAME]);
                    string ruleName = ((FieldLookupValue)item[Constants.AppListDetailsColumns.SECURITY_CHECK]).LookupValue;
                    appList.SecurityCheck = appList[ruleName];
                    appList.IsPrimary = Convert.ToBoolean(item[Constants.AppListDetailsColumns.PRIMARY]);
                    appList.IsActive = Convert.ToBoolean(item[Constants.AppListDetailsColumns.ACTIVE]);

                    appListDetails.Add(appList);
                }

                return appListDetails;
            }
            catch (ApplicationPermissionDetailsException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApplicationListDetailsException(Constants.CustomExceptionMessages.LIST_DETAILS, ex);
            }
        }
    }
}
