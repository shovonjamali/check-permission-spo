using PermissionCheckerDaemon.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Console;
using Microsoft.SharePoint.Client;
using PermissionCheckerDaemon.Configuration;
using System.Threading.Tasks;
using System.Diagnostics;
using PermissionCheckerDaemon.Interfaces;


namespace PermissionCheckerDaemon.Services
{
    class PermissionDeterminant : IPermissionChecker
    {
        private Helper _helper;
        private General _svcGeneral;
        private Dictionary<string, int> _permissionOrder = new Dictionary<string, int>();
        private List<string> _siteUrls;
        public PermissionDeterminant()
        {
            _helper = new Helper();
            _svcGeneral = new General();
        }

        public List<AppDetails> CheckPermission(IEnumerable<IGrouping<string, AppDetails>> grouped_by_site)
        {
            var apps = new List<AppDetails>();
            var principal_app = new AppDetails();

            var stopWatch = Stopwatch.StartNew();
            foreach (var site in grouped_by_site)
            {
                WriteLine($"\nSite: {site.Key}");
                foreach (var app in site)
                {
                    try
                    {
                        _permissionOrder = app.PermissionOrder;

                        //using (ClientContext context = _svcGeneral.GetContext(site.Key, app.ClientID, app.ClientSecret))
                        using (ClientContext context = _svcGeneral.GetContextUsingCertificate(site.Key))
                        {
                            WriteLine($"\tApplication: {app.ApplicationName}");
                            principal_app = _helper.MapPrincipalApp(app);

                            foreach (var list in app.AppLists)
                            {
                                WriteLine($"\t\tList/Library: {list.ListName}");

                                try
                                {
                                    if (list.SecurityCheck.PermissionLevel == Constants.AppPermissionDetailsColumns.PermissionLevelValues.SITE_LEVEL)
                                    {
                                        List<string> allSiteUrls = GetAllSiteUrls(context, list);

                                        foreach (string siteUrl in allSiteUrls)
                                        {
                                            using (ClientContext clientContext = _svcGeneral.GetContextUsingCertificate(siteUrl))
                                            {
                                                AppListDetails _list = new AppListDetails();
                                                _list.SecurityCheck = list.SecurityCheck;
                                                principal_app.AppLists.Add(CheckSiteLevelPermission(clientContext, _list, list.SecurityCheck.AccountsToBeVerified));
                                            }
                                        }

                                    }
                                    else if (list.SecurityCheck.PermissionLevel == Constants.AppPermissionDetailsColumns.PermissionLevelValues.ITEM_LEVEL)
                                    {
                                        string calculatingDate = _helper.GetCalculatingDate();
                                        string viewXml = _helper.GetModifiedItemQuery(calculatingDate, 4000);
                                        List<ListItem> itemCollection = _svcGeneral.GetAllItems(context, list.ListName, viewXml);
                                        principal_app.AppLists.Add(CheckItemLevelPermission(context, list, itemCollection, list.SecurityCheck.AccountsToBeVerified));
                                    }
                                    else
                                    {
                                        principal_app.AppLists.Add(CheckListLevelPermission(context, list, list.SecurityCheck.AccountsToBeVerified));
                                    }
                                }
                                catch (ArgumentException e)
                                {
                                    ErrorInfo.SecondaryErrorMessage += e.Message;
                                }
                                catch (Exception ex)
                                {
                                    ErrorInfo.SecondaryErrorMessage += $"\nError Occured in PermissionCheckerDaemon.CheckPermission(), \nError Messagae-> {ex.Message} \nError Line-> {ex.StackTrace}";
                                }
                            }
                        }

                        apps.Add(principal_app);
                    }
                    catch (Exception ex)
                    {
                        ErrorInfo.SecondaryErrorMessage += $"\nError Occured in PermissionCheckerDaemon.CheckPermission() while accessing site {site.Key}, \nError Messagae-> {ex.Message} \nError Line-> {ex.StackTrace}";
                    }                    
                }
            }

            WriteLine("\nPermission checking completed");
            WriteLine("\nforeach loop execution time = {0} seconds\n", stopWatch.Elapsed.TotalSeconds);
            return apps;
        }

        private List<string> GetAllSiteUrls(ClientContext context, AppListDetails list)
        {
            _siteUrls = new List<string>();
            _siteUrls.Add(context.Url);
            GetAllSiteAddress(context, context.Web);
            var exceptedSites = (list.ListName).Split(',');
            List<string> _exceptedSites = new List<string>();
            _exceptedSites = exceptedSites.ToList();
            return _siteUrls.Except(_exceptedSites).ToList();
        }

        #region Parallel block
        /*
        public List<AppDetails> CheckPermissionParallel(IEnumerable<IGrouping<string, AppDetails>> grouped_by_site)
        {
            var apps = new List<AppDetails>();
            var principal_app = new AppDetails();

            var stopWatch = Stopwatch.StartNew();
            Parallel.ForEach(grouped_by_site, site =>
            {
                using (ClientContext context = _svcGeneral.GetContext(site.Key))
                {
                    WriteLine($"\nSite: {site.Key}");
                    foreach (var app in site)
                    {
                        WriteLine($"\tApplication: {app.ApplicationName}");
                        principal_app = _helper.MapPrincipalApp(app);

                        foreach (var list in app.AppLists)
                        {
                            WriteLine($"\t\tList/Library: {list.ListName}");
                            //if (list.SecurityCheck.PermissionLevel == Constants.AppPermissionDetailsColumns.PermissionLevelValues.ITEM_LEVEL)
                            //{
                            //    string calculatingDate = GetCalculatingDate();
                            //    string viewXml = _helper.GetModifiedItemQuery(calculatingDate, 4000);
                            //    List<ListItem> itemCollection = _svcGeneral.GetAllItems(context, list.ListName, viewXml);
                            //    principal_app.AppLists.Add(CheckItemLevelPermission(context, list, itemCollection));
                            //}
                            //else
                            //{
                            //    var a = _svcGeneral.GetListPermission(list.ListName, context);
                            //    // To Do
                            //    //principal_app.AppLists.Add();
                            //}
                        }
                    }
                    apps.Add(principal_app);
                }
            });

            WriteLine("\nParallel.ForEach() execution time = {0} seconds", stopWatch.Elapsed.TotalSeconds);
            return apps;
        }
        */
        #endregion

        /// <summary>
        /// This funtion is being used to check the site level permission
        /// This funtion will work for both Site Collection and Sub Site
        /// This funtion check if a site is a sub site or not where it will only check the scope level error only for Sub Site
        /// </summary>
        /// <param name="clientContext"></param>
        /// <param name="list"></param>
        /// <param name="accountsToBeVerified"></param>
        /// <returns>AppListDetails</returns>
        /*private AppListDetails CheckSiteLevelPermission(ClientContext clientContext, AppListDetails list, List<string> accountsToBeVerified)
        {
            try
            {
                list.ListLevelInfo.PermissionUrl = clientContext.Url + "/_layouts/15/user.aspx";

                bool _isSubSite = clientContext.Web.IsSubSite();
                if (_isSubSite)
                {
                    clientContext.Load(clientContext.Web, i => i.HasUniqueRoleAssignments);
                    clientContext.ExecuteQuery();
                    bool _hasUniquePermission = clientContext.Web.HasUniqueRoleAssignments;
                    if (_helper.HasScopeError(list.SecurityCheck.PermissionScope, _hasUniquePermission))
                    {
                        list.ListLevelInfo.HasError = true;
                        list.ListLevelInfo.ErrorType = Constants.ErrorLogDetails.SiteErrorTypeValue.SCOPE_ERROR;
                        return list;
                    }
                }

                Dictionary<string, MemberDetails> sitePermissions = _svcGeneral.GetPermissionDetails(clientContext, clientContext.Web);

                foreach (string member in accountsToBeVerified)
                {
                    List<string> _permission = PermissionLevelExtension.GetPrincipalsRoleDefinitions(sitePermissions, clientContext, member);

                    if (!_helper.HasExpectedPermission(_permission, list.SecurityCheck.PermissionType, _permissionOrder))
                    {
                        list.ListLevelInfo.HasError = true;
                        list.ListLevelInfo.ErrorType = Constants.ErrorLogDetails.SiteErrorTypeValue.ROLE_ERROR;
                        break;
                    }
                }

                return list;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"\nError Occured in PermissionCheckerDaemon.CheckSiteLevelPermission(), \nError Messagae-> {ex.Message} \nError Line-> {ex.StackTrace}");
            }          
        }*/


        private AppListDetails CheckSiteLevelPermission(ClientContext clientContext, AppListDetails list, List<string> accountsToBeVerified)
        {
            try
            {
                list.ListLevelInfo.PermissionUrl = clientContext.Url + "/_layouts/15/user.aspx";

                clientContext.Load(clientContext.Web, i=> i.Title, i => i.HasUniqueRoleAssignments);
                clientContext.ExecuteQuery();

                list.ListName = clientContext.Web.Title;

                if (clientContext.Web.IsSubSite())
                {
                    //clientContext.Load(clientContext.Web, i => i.HasUniqueRoleAssignments);
                    //clientContext.ExecuteQuery();
                    bool _hasUniquePermission = clientContext.Web.HasUniqueRoleAssignments;
                    if (_helper.HasScopeError(list.SecurityCheck.PermissionScope, _hasUniquePermission))
                    {
                        list.ListLevelInfo.HasError = true;
                        list.ListLevelInfo.ErrorType = Constants.ErrorLogDetails.SiteErrorTypeValue.SCOPE_ERROR;
                        return list;
                    }
                }

                Dictionary<string, MemberDetails> sitePermissions = _svcGeneral.GetPermissionDetails(clientContext, clientContext.Web);

                foreach (string member in accountsToBeVerified)
                {
                    List<string> _permission = PermissionLevelExtension.GetPrincipalsRoleDefinitions(sitePermissions, clientContext, member);

                    if (!_helper.HasExpectedPermission(_permission, list.SecurityCheck.PermissionType, _permissionOrder))
                    {
                        list.ListLevelInfo.HasError = true;
                        list.ListLevelInfo.ErrorType = Constants.ErrorLogDetails.SiteErrorTypeValue.ROLE_ERROR;
                        break;
                    }
                }

                return list;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"\nError Occured in PermissionCheckerDaemon.CheckSiteLevelPermission(), \nError Messagae-> {ex.Message} \nError Line-> {ex.StackTrace}");
            }
        }

        private void GetAllSiteAddress(ClientContext clientContext, Web web)
        {
            clientContext.Load(web, x => x.Webs, x => x.Title);
            clientContext.ExecuteQuery();
            foreach (Web subWeb in web.Webs)
            {
                _siteUrls.Add(subWeb.Url);
                GetAllSiteAddress(clientContext, subWeb);
            }
        }

        private AppListDetails CheckListLevelPermission(ClientContext clientContext, AppListDetails list, List<string> accountsToBeVerified)
        {
            try
            {
                List spList = clientContext.Web.Lists.GetByTitle(list.ListName);
                clientContext.Load(spList, l => l.HasUniqueRoleAssignments);
                clientContext.ExecuteQueryRetry();

                bool hasUniquePermission = spList.HasUniqueRoleAssignments;
                string expactedPermissionName = list.SecurityCheck.PermissionType;
                Dictionary<string, MemberDetails> listPermissions = _svcGeneral.GetPermissionDetails(clientContext, spList);
                string _spListId = _svcGeneral.GetListID(clientContext, list.ListName);

                list.ListLevelInfo.PermissionUrl = clientContext.Url + "/_layouts/15/user.aspx?obj=" + _spListId + ",list &List=" + _spListId;
                
                if(_helper.HasScopeError(list.SecurityCheck.PermissionScope, hasUniquePermission))
                {
                    list.ListLevelInfo.HasError = true;
                    list.ListLevelInfo.ErrorType = Constants.ErrorLogDetails.ListErrorTypeValue.SCOPE_ERROR;
                }
                else
                {
                    foreach (string member in accountsToBeVerified)
                    {
                        List<string> _permission = PermissionLevelExtension.GetPrincipalsRoleDefinitions(listPermissions, clientContext, member);
                        
                        if (!_helper.HasExpectedPermission(_permission, expactedPermissionName, _permissionOrder))
                        {
                            list.ListLevelInfo.HasError = true;
                            list.ListLevelInfo.ErrorType = Constants.ErrorLogDetails.ListErrorTypeValue.ROLE_ERROR;
                            break;
                        }
                    }
                }

                return list;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"\nError Occured in PermissionCheckerDaemon.CheckListLevelPermission(), \nError Messagae-> {ex.Message} \nError Line-> {ex.StackTrace}");
            }
        }

        private AppListDetails CheckItemLevelPermission(ClientContext clientContext, AppListDetails list, List<ListItem> itemCollection, List<string> accountsToBeVerified)
        {
            string _spListId = _svcGeneral.GetListID(clientContext, list.ListName);
            string expactedPermissionName = list.SecurityCheck.PermissionType;

            foreach (ListItem item in itemCollection)
            {
                try
                {
                    clientContext.Load(item, i => i.HasUniqueRoleAssignments);
                    clientContext.ExecuteQuery();
                    list.AppTracer.IdChecked.Add(item.Id);
                    
                    RequestItem requestItem = new RequestItem();
                    requestItem.Title = Convert.ToString(item["Title"]);
                    requestItem.ItemId = item.Id;
                    requestItem.PermissionUrl = clientContext.Url + "/_layouts/15/user.aspx?List=" + _spListId + "&obj=" + _spListId + "," + item.Id + ",LISTITEM";
                    
                    if (_helper.HasScopeError(list.SecurityCheck.PermissionScope, item.HasUniqueRoleAssignments))
                    {
                        requestItem.ErrorType = Constants.ErrorLogDetails.ItemErrorTypeValue.SCOPE_ERROR;
                        list.AppTracer.ErrorsOccurred.Add(item.Id);
                    }
                    else
                    {
                        Dictionary<string, MemberDetails> listItemsPermissions = _svcGeneral.GetPermissionDetails(clientContext, item);
                        foreach (string member in accountsToBeVerified)
                        {
                            try
                            {
                                List<string> _permission = PermissionLevelExtension.GetPrincipalsRoleDefinitions(listItemsPermissions, clientContext, member);

                                if (!_helper.HasExpectedPermission(_permission, expactedPermissionName, _permissionOrder))
                                {
                                    list.AppTracer.ErrorsOccurred.Add(item.Id);
                                    requestItem.ErrorType = Constants.ErrorLogDetails.ItemErrorTypeValue.ROLE_ERROR;
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                ErrorInfo.SecondaryErrorMessage += $"\nError Occured in PermissionCheckerDaemon.CheckItemLevelPermission()->HasPermission(), \nItem ID-->{item.Id} \nError Messagae-> {ex.Message} \nError Line-> {ex.StackTrace}";
                            }
                        }
                    }

                    list.AppTracer.InspectingItems.Add(requestItem);
                }
                catch (Exception ex)
                {
                    ErrorInfo.SecondaryErrorMessage += $"\nError Occured in PermissionCheckerDaemon.CheckItemLevelPermission(), \nItem ID-->{item.Id} \nError Messagae-> {ex.Message} \nError Line-> {ex.StackTrace}";
                }
            }

            list.ListLevelInfo.HasError = false;

            return list;
        }
    }
}