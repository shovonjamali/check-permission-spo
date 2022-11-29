using System.Collections.Generic;
using PermissionCheckerDaemon.Entities;
using System.Linq;
using System.Configuration;
using static System.Console;
using System;
using System.Globalization;
using PermissionCheckerDaemon.Configuration;

namespace PermissionCheckerDaemon.Services
{
    class Helper
    {
        internal string GetActiveItemQuery()
        {
            string viewXml = string.Format(@"
                                            <View>
                                                <Query>
                                                    <Where>
                                                      <Eq>
                                                         <FieldRef Name='Active' />
                                                         <Value Type='Boolean'>1</Value>
                                                      </Eq>
                                                    </Where>
                                                </Query>
                                            </View>");
            return viewXml;
        }

        internal string GetModifiedItemQuery(string calculatingDateString, int rowLimit)
        {
            string viewXml = string.Format($@"<View Scope='Recursive'>
                                                <Query>
                                                    <Where>
                                                        <Geq>
                                                            <FieldRef Name='Modified' />
                                                            <Value Type='DateTime'>{calculatingDateString}</Value>
                                                        </Geq>
                                                    </Where>
                                                </Query>
                                                <RowLimit>{rowLimit}</RowLimit>
                                            </View>");
            return viewXml;
        }

        internal bool HasFaultyItem(List<AppDetails> checked_apps)
        {
            var error_occured_items = checked_apps.Count(app => app.AppLists.Count(l => l.AppTracer.ErrorsOccurred.Count != 0) != 0) == 0 ? false : true;

            var error_occured_lists = checked_apps.Count(app => app.AppLists.Count(l => l.ListLevelInfo.HasError != false) != 0) == 0 ? false : true;

            if (!error_occured_items && !error_occured_lists)
                return false;

            return true;
        }            

        internal AppDetails MapPrincipalApp(AppDetails app)
        {
            var prinicpal_app = new AppDetails();

            prinicpal_app.ApplicationName = app.ApplicationName;
            prinicpal_app.IsActive = app.IsActive;
            prinicpal_app.SiteURL = app.SiteURL;
            prinicpal_app.ErrorEmailTo = app.ErrorEmailTo;
            prinicpal_app.ErrorEmailCC = app.ErrorEmailCC;
            
            return prinicpal_app;
        }

        internal string GetCalculatingDate()
        {
            int runPeriod = Convert.ToInt32(ConfigurationManager.AppSettings["DataModificationInterval"]);
            string format = "yyyy-MM-dd";
            string calculatingDate = DateTime.Today.AddDays(-runPeriod).ToString(format, DateTimeFormatInfo.InvariantInfo);
            return calculatingDate;
        }

        internal void DumpResult(IEnumerable<IGrouping<string, AppDetails>> grouped_by_site)
        {
            foreach (var group in grouped_by_site)
            {
                WriteLine($"\nSite: {group.Key}");
                foreach (var app in group)
                {
                    WriteLine($"\tApplication: {app.ApplicationName}");
                    foreach (var list in app.AppLists)
                    {
                        WriteLine($"\t\tList/Library: {list.ListName}");
                        #region Email Sample
                        //foreach (var processed_item in list.AppTracer.InspectingItems)
                        //{
                        //    string siteUrl = app.SiteURL;
                        //    string app_name = app.ApplicationName;
                        //    string list_name = list.ListName;
                        //    string id = processed_item.ItemId.ToString();
                        //    string error_type = processed_item.ErrorType;
                        //    string item_url = processed_item.ItemUrl;
                        //}
                        #endregion
                    }
                }
            }
            WriteLine();
        }

        internal bool EmailSendingStatus(EmailObject emailObject) 
        {
            bool disableSuccessEmail = Convert.ToBoolean(ConfigurationManager.AppSettings["DisableSuccessEmail"]);

            if (disableSuccessEmail && !emailObject.Content.SetImportance)
                return false;

            return true;
        }

        internal bool HasExpectedPermission(List<string> _permission, string expactedPermissionName, Dictionary<string, int> dicPermissionOrder)
        {
            bool _hasExpectedPermission = true;

            if (_permission.Count > 1)
                _hasExpectedPermission = PermissionLevelExtension.PermissionOrder(_permission, expactedPermissionName, dicPermissionOrder);
               // _hasExpectedPermission = false;
            else if(_permission.Count == 1 && _permission[0] != expactedPermissionName)
                _hasExpectedPermission = false;
            else if (expactedPermissionName == Constants.AppPermissionDetailsColumns.PermissionTypeValues.NO_ACCESS && _permission.Count != 0)
                _hasExpectedPermission = false;
            else if (expactedPermissionName != Constants.AppPermissionDetailsColumns.PermissionTypeValues.NO_ACCESS && _permission.Count == 0)
                _hasExpectedPermission = false;
            return _hasExpectedPermission;
        }

        internal bool HasScopeError(string permissionScope, bool hasUniquePermission)
        {
            bool _hasError = false;

            if (permissionScope == Constants.AppPermissionDetailsColumns.PermissionScopeValues.UNIQUE && !hasUniquePermission)
                _hasError = true;
            else if(permissionScope == Constants.AppPermissionDetailsColumns.PermissionScopeValues.INHERITED && hasUniquePermission)
                _hasError = true;

            return _hasError;
        }
    }
}