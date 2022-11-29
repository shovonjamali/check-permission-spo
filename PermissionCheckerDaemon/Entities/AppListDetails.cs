using Microsoft.SharePoint.Client;
using System.Collections.Generic;
using PermissionCheckerDaemon.Configuration;
using PermissionCheckerDaemon.Services;
using System;
using PermissionCheckerDaemon.Exceptions;
using System.Linq;

namespace PermissionCheckerDaemon.Entities
{
    class AppListDetails
    {
        public string ApplicationName { get; set; } = string.Empty;
        public string ListName { get; set; } = string.Empty;
        public AppPermissionDetails SecurityCheck { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsActive { get; set; }
        public AppLog AppTracer { get; set; } = new AppLog();
        public ListLevelInfo ListLevelInfo { get; set; } = new ListLevelInfo();

        private General _svcGeneral;
        private List<AppPermissionDetails> _appPermissionDetails;

        public AppListDetails() { }

        public AppListDetails(ClientContext context)
        {
            _svcGeneral = new General();

            _appPermissionDetails = GetPermissionDefinitions(context);
        }

        public AppPermissionDetails this[string ruleName]
        {
            get
            {
                // Returning the permission details associated with this list based on the rule name
                return _appPermissionDetails.Where(p => p.RuleName == ruleName).FirstOrDefault();
            }
        }

        private List<AppPermissionDetails> GetPermissionDefinitions(ClientContext context)
        {
            var appPermissionDetails = new List<AppPermissionDetails>();
                        
            try
            {
                // Getting all the items from SST-ApplicationPermissionDetails list
                var items_app_permissions = _svcGeneral.GetAllItems(context, listName: Constants.AppLists.APPLICATION_PERMISSION_DETAILS);
                //throw new Exception();
                foreach (var item in items_app_permissions)
                {
                    var permissionDefinition = new AppPermissionDetails();

                    permissionDefinition.RuleName = Convert.ToString(item[Constants.AppPermissionDetailsColumns.RULE_NAME]);
                    permissionDefinition.AccountsToBeVerified = _svcGeneral.GetAllPricipalName(context, Convert.ToString(item[Constants.AppPermissionDetailsColumns.ACCOUNTS_TO_BE_VERIFIED]));
                    permissionDefinition.PermissionType = Convert.ToString(item[Constants.AppPermissionDetailsColumns.PERMISSION_TYPE]);
                    permissionDefinition.PermissionLevel = Convert.ToString(item[Constants.AppPermissionDetailsColumns.PERMISSION_LEVEL]);
                    permissionDefinition.PermissionScope = Convert.ToString(item[Constants.AppPermissionDetailsColumns.PERMISSIONSCOPE]);

                    appPermissionDetails.Add(permissionDefinition);
                }

                return appPermissionDetails;
            }
            catch (Exception ex)
            {
                throw new ApplicationPermissionDetailsException(Constants.CustomExceptionMessages.PERMISSION_DETAILS, ex);
            }                            
        }
    }
}