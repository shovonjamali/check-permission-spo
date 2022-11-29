using System.Collections.Generic;
using Microsoft.SharePoint.Client;
using System.Linq;
using System.Collections;
using System;
using PermissionCheckerDaemon.Entities;
using System.Diagnostics;

namespace PermissionCheckerDaemon.Services
{
    class General
    {
        private AuthenticationAssistant _authenticationAssistant = null;

        public General()
        {
            _authenticationAssistant = new AuthenticationAssistant();
        }

        [Obsolete]
        internal ClientContext GetContext(string siteUrl) => _authenticationAssistant.GetContextUsingACS(siteUrl);

        [Obsolete]
        internal ClientContext GetContext(string siteUrl, string clientID, string clientSecret) => _authenticationAssistant.GetContextUsingACS(siteUrl, clientID, clientSecret);

        internal ClientContext GetContextUsingCertificate(string siteUrl)
        {
            string localPath = @"E:\Tahmid\Codebase\ESS\TestConsole\SPSecurityTool\SPSecurityTool.pfx";
            string azurePath = @"D:\home\SPSecurityTool.pfx";

            string password = "CDNS123";

            string path = Debugger.IsAttached ? localPath : azurePath;

            return _authenticationAssistant.GetContextUsingAzureADAppUsingCertificateFromDirectory(siteUrl, path, password);
        }

        internal List<ListItem> GetAllItems(ClientContext context, string listName, string viewXml = "")
        {
            List list = context.Web.Lists.GetByTitle(listName);
            List<ListItem> items = new List<ListItem>();
            int rowLimit = 1000;

            if (viewXml == "")
            {
                viewXml = string.Format(@"
                                         <View Scope='Recursive'>                
                                            <RowLimit>{0}</RowLimit>
                                         </View>", rowLimit);
            }

            var camlQuery = new CamlQuery();
            camlQuery.ViewXml = viewXml;

            do
            {
                ListItemCollection listItemCollection = list.GetItems(camlQuery);
                context.Load(listItemCollection);
                context.ExecuteQueryRetry();

                //-- Adding the current set of ListItems in single buffer
                items.AddRange(listItemCollection);

                //-- Reset the current pagination info
                camlQuery.ListItemCollectionPosition = listItemCollection.ListItemCollectionPosition;

            } while (camlQuery.ListItemCollectionPosition != null);

            // WriteLine($"Items: {items.Count}");
            return items;
        }

        internal ListItem GetItemById(ClientContext context, int itemId, string listName)
        {
            var spList = context.Web.Lists.GetByTitle(listName);
            ListItem item = spList.GetItemById(itemId);

            context.Load(item);
            context.Load(item, II => II.HasUniqueRoleAssignments);
            context.Load(item, II => II.RoleAssignments);
            context.ExecuteQueryRetry();

            return item;
        }

        internal string GetListID(ClientContext clientContext, string listName)
        {
            string _listId = string.Empty;
            try
            {
                List _spList = clientContext.Web.Lists.GetByTitle(listName);
                clientContext.Load(_spList);
                clientContext.ExecuteQueryRetry();
                _listId = _spList.Id.ToString();
            }
            catch (Exception ex)
            {
                _listId = "";               
                ErrorInfo.SecondaryErrorMessage += $"\nError Occured in General.GetListID(), \nListName->{listName} \nError Messagae-> {ex.Message} \nError Line-> {ex.StackTrace}";
            }
            
            return _listId;
        }

        internal string GetItemLink(ClientContext context, string siteUrl, string listName, int itemId) =>
            siteUrl + "/Lists/" + listName + "/DispForm.aspx?ID=" + itemId;

        internal List<string> GetAllPricipalName(ClientContext clientContext, string accountToBeVerified)
        {
            List<string> _allPrincipleName = new List<string>();
            string[] members = accountToBeVerified.Split(';');

            foreach (string member in members)
            {
                try
                {
                    //if (member.Contains("@"))
                    //{
                    //    User spUser = clientContext.Web.SiteUsers.GetByEmail(member.Trim());
                    //    clientContext.Load(spUser);
                    //    clientContext.ExecuteQuery();

                    //    _allPrincipleName.Add(spUser.Title);
                    //}
                    //else
                    //{
                    //    _allPrincipleName.Add(member.Trim());
                    //}
                    _allPrincipleName.Add(member.Trim());

                }
                catch (Exception ex)
                {
                    ErrorInfo.PrimaryErrorMessage += "\nError From General.GetAllPricipalName(). Error Details:" + ex.Message;
                }
            }

            return _allPrincipleName;
        }

        /// <summary>    
        /// This funtion get the site/list/list item permission details. And return it by a dictonary.    
        /// </summary>    
        /// <param name="clientContext">type ClientContext</param>    
        /// <param name="securableObject">type SecurableObject e.g. web/list/list item</param>    
        /// <returns>return type is Dictionary<string, MemberDetails></returns>    
        internal Dictionary<string, MemberDetails> GetPermissionDetails(ClientContext clientContext, SecurableObject securableObject)
        {
            try
            {
                IQueryable<RoleAssignment> queryString = securableObject.RoleAssignments.Include(roleAsg => roleAsg.Member,
                                                                                       roleAsg => roleAsg.RoleDefinitionBindings.Include(roleDef => roleDef.Name));
                IEnumerable roles = clientContext.LoadQuery(queryString);
                clientContext.ExecuteQueryRetry();

                //Dictionary<string, string> permisionDetails = new Dictionary<string, string>();
                Dictionary<string, MemberDetails> permisionDetails = new Dictionary<string, MemberDetails>();
                foreach (RoleAssignment ra in roles)
                {
                    var rdc = ra.RoleDefinitionBindings;
                    string permission = string.Empty;
                    foreach (var rdbc in rdc)
                    {
                        permission += rdbc.Name.ToString() + ",";
                    }
                    
                    if (!permisionDetails.ContainsKey(ra.Member.Title))
                    {
                        MemberDetails memberDetails = new MemberDetails(ra.Member.PrincipalType.ToString(), permission);
                        permisionDetails.Add(ra.Member.Title, memberDetails);
                    }
                    else
                    {
                        MemberDetails memberDetails = permisionDetails[ra.Member.Title];
                        memberDetails.UpdatePermission(permission);
                        permisionDetails[ra.Member.Title] = memberDetails;
                    }
                    
                }

                return permisionDetails;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal List<string> GetEmailAddressList(string emailAddress)
        {
            var emailAddressesList = new List<string>();

            string[] splitEmails = emailAddress.Split(';');
            foreach (string email in splitEmails)
            {
                if (email != string.Empty)
                {
                    emailAddressesList.Add(email);
                }
            }

            return emailAddressesList.Distinct().ToList();
        }

        //internal Dictionary<string, string> GetListPermission(string listName, ClientContext clientContext, out bool hasUniquePermission)
        //{
        //    List list = clientContext.Web.Lists.GetByTitle(listName);
        //    clientContext.Load(list, l=>l.HasUniqueRoleAssignments);
        //    clientContext.ExecuteQueryRetry();

        //    hasUniquePermission = list.HasUniqueRoleAssignments;

        //    IQueryable<RoleAssignment> queryForList = list.RoleAssignments.Include(roleAsg => roleAsg.Member,
        //                                                                           roleAsg => roleAsg.RoleDefinitionBindings.Include(roleDef => roleDef.Name));
        //    Dictionary<string, string> listPermissions = GetPermissionDetails(clientContext, queryForList);

        //    return listPermissions;
        //}

        //internal Dictionary<string, string> GetListItemPermission(SecurableObject securableObject, ClientContext clientContext)
        //{  
        //    IQueryable<RoleAssignment> queryForListItem = securableObject.RoleAssignments.Include(roleAsg => roleAsg.Member,
        //                                                                               roleAsg => roleAsg.RoleDefinitionBindings.Include(roleDef => roleDef.Name));
        //    Dictionary<string, string> itemPermissionCollection = GetPermissionDetails(clientContext, queryForListItem);

        //    return itemPermissionCollection;
        //}

        //internal bool HasPermission(Dictionary<string, string> dictionary, string member, string expactedPermissionName, ClientContext clientContext)
        //{
        //    bool _permissionValidation = false;
        //    string principleName = string.Empty;
        //    string userLoginName = string.Empty;
        //    string actualPermissionName = "";
        //    bool isUser;
        //    try
        //    {

        //        principleName = GetPrincipleName(member, clientContext, out isUser, out userLoginName);

        //        bool _hasPermission = dictionary.TryGetValue(principleName, out actualPermissionName);

        //        if (isUser)
        //        {

        //        }
        //        if (expactedPermissionName != Constants.AppPermissionDetailsColumns.PermissionTypeValues.NO_ACCESS)
        //        {
        //            string[] _permissions = actualPermissionName.Split(',');
        //            if (_permissions.Length == 2 && _permissions[0] == expactedPermissionName)
        //            {
        //                _permissionValidation = true;
        //            }
        //        }
        //        else if (expactedPermissionName == Constants.AppPermissionDetailsColumns.PermissionTypeValues.NO_ACCESS && !_hasPermission)
        //        {
        //            _permissionValidation = true;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        _permissionValidation = false;
        //    }

        //    return _permissionValidation;
        //}

        //private static string GetPrincipleName(string member, ClientContext clientContext, out bool isUser, out string userLoginName)
        //{
        //    string _principleName = string.Empty;
        //    userLoginName = "";


        //    if (member.Contains("@"))
        //    {
        //        User spUser = clientContext.Web.SiteUsers.GetByEmail(member.Trim());
        //        clientContext.Load(spUser);
        //        clientContext.ExecuteQuery();
        //        _principleName = spUser.Title;
        //        isUser = true;
        //        userLoginName = spUser.LoginName;
        //    }
        //    else
        //    {
        //        _principleName = member;
        //        isUser = false;

        //    }

        //    return _principleName;
        //}


    }
}