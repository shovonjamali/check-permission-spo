using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using PermissionCheckerDaemon.Entities;
using PermissionCheckerDaemon.Configuration;

namespace PermissionCheckerDaemon.Services
{
    public static class PermissionLevelExtension
    {
        /// <summary>
        /// This function returns
        /// collection of permission level
        /// of an user/group 
        /// for a web/list/list item
        /// </summary>
        /// <param name="roleAssignments">Dictonary<string string></param>
        /// <param name="context">ClientContext</param>
        /// <param name="principalName">user (email/login)/group name</param>
        /// <returns></returns>
        public static List<string> GetPrincipalsRoleDefinitions(Dictionary<string, MemberDetails> roleAssignments, ClientContext context, string principalName)
        {
            var permissionDefinitions = new List<string>();

            var principal = ResolvePrincipal(context, principalName);

            //var roleAssignments = GetRoleAssignments(context, securableObject);

            var directPermissionLevel = roleAssignments.Where(r => r.Key == principal.Title).FirstOrDefault();
            try
            {
                permissionDefinitions.AddRange(directPermissionLevel.Value.Permission.TrimEnd(',').Split(','));
            }
            catch (Exception e)
            {
                //throw;
            }

            if (principal.PrincipalType == PrincipalType.User)
            {
                var user = context.Web.EnsureUser(principalName);
                context.Load(user, u => u.LoginName);
                context.ExecuteQueryRetry();

                var groups = GetUserGroups(context, user.LoginName);

                var userGroupsPermissionLevel = (from g in groups
                                                 join r in roleAssignments
                                                 on g equals r.Key
                                                 select new
                                                 {
                                                     GroupName = g,
                                                     PermissionLevel = r.Value.Permission
                                                 }).ToList();

                foreach (var r in userGroupsPermissionLevel)
                {
                    try
                    {
                        permissionDefinitions.AddRange(r.PermissionLevel.TrimEnd(',').Split(','));
                    }
                    catch (Exception ex)
                    {
                        //throw
                    }
                }
            }

            var _a = SecurityGroupPermissions(roleAssignments, context);
            permissionDefinitions.AddRange(_a);

            // To handle Limited Access
            return IgnoreLimitedAccessPermissionLevel(permissionDefinitions);
            //return permissionDefinitions.Distinct().ToList();
        }

        private static List<string> IgnoreLimitedAccessPermissionLevel(List<string> permissionDefinitions)
        {
            permissionDefinitions = permissionDefinitions.Distinct().ToList();

            var itemToRemove = permissionDefinitions.SingleOrDefault(pd => pd == "Limited Access");

            if (itemToRemove != null)
                permissionDefinitions.Remove(itemToRemove);

            return permissionDefinitions.Distinct().ToList();
        }

        private static List<string> SecurityGroupPermissions(Dictionary<string, MemberDetails> roleAssignments, ClientContext clientContext)
        {
            List<string> _permissionDefinitions = new List<string>();
            foreach (var r in roleAssignments)
            {
                
                if (r.Key == Constants.SecurityGroups.EVERYONE)
                {
                    _permissionDefinitions.AddRange(r.Value.Permission.TrimEnd(',').Split(','));
                }
                else if (r.Key == Constants.SecurityGroups.EVERYONE_EXCEPT_EXTERNAL_USERS)
                {
                    _permissionDefinitions.AddRange(r.Value.Permission.TrimEnd(',').Split(','));
                }
                else if (r.Value.PrincipleType == "SharePointGroup")
                {
                    Group group = clientContext.Web.SiteGroups.GetByName(r.Key);
                    clientContext.Load(group, grp => grp.Users);
                    clientContext.ExecuteQuery();
                    foreach(User user in group.Users)
                    {
                        if(user.Title == Constants.SecurityGroups.EVERYONE || user.Title == Constants.SecurityGroups.EVERYONE_EXCEPT_EXTERNAL_USERS)
                        {
                            _permissionDefinitions.AddRange(r.Value.Permission.TrimEnd(',').Split(','));
                        }
                    }
                }
            }

            return _permissionDefinitions; 
        }
        
        internal static bool PermissionOrder(List<string> permissions, string expactedPermissionName, Dictionary<string, int> dicPermissionOder)
        {
            Dictionary<string, int> _permissionOder = new Dictionary<string, int>();
            //_permissionOder.Add("Read", 1);
            //_permissionOder.Add("Contribute", 2);
            //_permissionOder.Add("Full Control", 5);

            bool _hasExpectedPermission = false;

            foreach (string p in permissions)
            {
                if (dicPermissionOder.TryGetValue(p, out int orderNo))
                {
                    _permissionOder.Add(p, orderNo);
                }
            }

            if (_permissionOder.TryGetValue(expactedPermissionName, out int _expectedOrderNo))
            {
                int _heigherPermission = 0;
                _heigherPermission = _permissionOder.Where(r => r.Value > _expectedOrderNo).Select(r=> r.Value).FirstOrDefault();

                if (_heigherPermission == 0)
                    _hasExpectedPermission = true;

            }
            return _hasExpectedPermission; 
        }
        private static List<string> GetUserGroups(ClientContext context, string userLogin)
        {
            User user = context.Web.SiteUsers.GetByLoginName(userLogin);
            GroupCollection groupColl = user.Groups;

            context.Load(groupColl);
            context.ExecuteQueryRetry();

            return  groupColl.Select(g => g.Title).ToList();   
        }

        //private static Dictionary<string, string> GetRoleAssignments(ClientContext context, SecurableObject securableObject)
        //{
        //    IQueryable<RoleAssignment> query = securableObject.RoleAssignments.Include(roleAsg => roleAsg.Member,
        //                                                                           roleAsg => roleAsg.RoleDefinitionBindings.Include(roleDef => roleDef.Name));

        //    Dictionary<string, string> assignments = GetRoleDefinitionBindings(context, query);

        //    return assignments;
        //}

        //private static Dictionary<string, string> GetRoleDefinitionBindings(ClientContext context, IQueryable<RoleAssignment> queryString)
        //{
        //    try
        //    {
        //        IEnumerable roles = context.LoadQuery(queryString);
        //        context.ExecuteQueryRetry();

        //        Dictionary<string, string> permisionDetails = new Dictionary<string, string>();
        //        foreach (RoleAssignment ra in roles)
        //        {
        //            var rdc = ra.RoleDefinitionBindings;
        //            string permissionLevel = string.Empty;

        //            foreach (var rdbc in rdc)
        //            {
        //                permissionLevel += rdbc.Name.ToString() + ",";
        //            }

        //            permisionDetails.Add(ra.Member.Title, permissionLevel);
        //        }

        //        return permisionDetails;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        private static Principal ResolvePrincipal(ClientContext context, string name)
        {
            Principal principal = null;

            try
            {
                ClientResult<PrincipalInfo> result = Utility.ResolvePrincipal(context, context.Web, name, PrincipalType.All, PrincipalSource.All, null, false);
                context.ExecuteQuery();

                if (result.Value.PrincipalType == PrincipalType.User)
                {
                    principal = context.Web.EnsureUser(result.Value.LoginName);
                }
                else if (result.Value.PrincipalType == PrincipalType.SecurityGroup || result.Value.PrincipalType == PrincipalType.SharePointGroup)
                {
                    if (result.Value.DisplayName == "")  // invalid input
                    {
                        return principal;
                    }
                    else
                    {
                        // sharepoint group -> principal type: Security Group, principal id: -1, login name: Anti-C Legal Admin Team, email: null, display name: Anti-C Legal Admin Team
                        if (result.Value.PrincipalId != -1)
                        {
                            principal = context.Web.SiteGroups.GetById(result.Value.PrincipalId);
                        }
                        // distribution list -> principal type: Security Group, principal id: -1, login name: c:0t.c|tenant|7ccb250c-7dc7-4005-8c8c-a3a8110f9823, email: Anti-c-asiapac-stg@abc.com, display name: Anti-c-asiapac-stg
                        // special group -> principal type: Security Group, principal id: -1, login name: c:0-.f|rolemanager|spo-grid-all-users/d36035c5-6ce6-4662-a3dc-e762e61ae4c9, email: null, display name: Everyone except external users
                        else
                        {
                            principal = context.Web.EnsureUser(result.Value.LoginName);
                        }
                    }
                }

                context.Load(principal);
                context.ExecuteQueryRetry();              
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);           
                principal = null;
            }

            return principal;
        }
    }
}
