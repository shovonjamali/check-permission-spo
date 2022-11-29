using PermissionCheckerDaemon.Interfaces;
using System.Collections.Generic;
using System.Linq;
using PermissionCheckerDaemon.Entities;
using PermissionCheckerDaemon.Configuration;

namespace PermissionCheckerDaemon.Mock
{
    class MockPermissionDeterminant : IPermissionChecker
    {
        public List<AppDetails> CheckPermission(IEnumerable<IGrouping<string, AppDetails>> grouped_by_site)
        {
            var apps = new List<AppDetails>();

            #region App1 
            var principal_app1 = new AppDetails();

            principal_app1.ApplicationName = "RTO CV Request";
            principal_app1.SiteURL = "https://abc.sharepoint.com/sites/WFO_RTO-dev";
            principal_app1.IsActive = true;
            principal_app1.ErrorEmailTo = "jamali@abc.com";
            principal_app1.ErrorEmailCC = "abedin@abc.com";

            #region App1 List1
            principal_app1.AppLists.Add(new AppListDetails()
            {
                ApplicationName = "RTO CV Request",
                ListName = "CustomerVisitRequest",
                IsPrimary = true,
                IsActive = true,
                AppTracer = new AppLog()
                {
                    IdChecked = { 10, 20, 30 },
                    //ErrorsOccurred = { 15 },
                    ErrorsOccurred = { },

                    InspectingItems = new List<RequestItem>()
                    {
                        new RequestItem()
                        {
                            ErrorType = "",
                            ItemId = 10,
                            PermissionUrl = "",
                            Title = "Title 10"
                        },
                        new RequestItem()
                        {
                            ErrorType = "",
                            ItemId = 15,
                            PermissionUrl = "",
                            Title = "Title 15"
                        },
                        new RequestItem()
                        {
                            ErrorType = "",
                            ItemId = 20,
                            PermissionUrl = "",
                            Title = "Title 20"
                        },
                        new RequestItem()
                        {
                            ErrorType = "",
                            ItemId = 30,
                            PermissionUrl = "",
                            Title = "Title 30"
                        }
                    }
                },
                SecurityCheck = new AppPermissionDetails()
                {
                    RuleName = "Unique Permission",                    
                    PermissionType = "No Access",
                    PermissionLevel = Constants.AppPermissionDetailsColumns.PermissionLevelValues.ITEM_LEVEL,
                    PermissionScope = Constants.AppPermissionDetailsColumns.PermissionScopeValues.UNIQUE,
                }
            });
            #endregion

            #region App1 List2
            principal_app1.AppLists.Add(new AppListDetails()
            {
                ApplicationName = "RTO CV Request",
                ListName = "EmailTemplates",
                IsPrimary = false,
                IsActive = true,
                AppTracer = new AppLog()
                {
                    IdChecked = { 1, 2 },
                    ErrorsOccurred = { },

                    InspectingItems = new List<RequestItem>()
                    {
                        new RequestItem()
                        {
                            ErrorType = "",
                            ItemId = 1,
                            PermissionUrl = "",
                            Title = "Title 1"
                        },
                        new RequestItem()
                        {
                            ErrorType = "",
                            ItemId = 2,
                            PermissionUrl = "",
                            Title = "Title 2"
                        }
                    }
                },
                ListLevelInfo = new ListLevelInfo()
                {
                    HasError = false,
                    ErrorType = Constants.ErrorLogDetails.ItemErrorTypeValue.ROLE_ERROR
                },
                SecurityCheck = new AppPermissionDetails()
                {
                    RuleName = "Read Permisssion",
                    PermissionType = "Read",
                    PermissionLevel = Constants.AppPermissionDetailsColumns.PermissionLevelValues.LIST_LEVEL,
                    PermissionScope = Constants.AppPermissionDetailsColumns.PermissionScopeValues.INHERITED,
                }
            });
            #endregion

            #endregion

            #region App2 
            var principal_app2 = new AppDetails();

            principal_app2.ApplicationName = "Provide A Gift";
            principal_app2.SiteURL = "https://abc.sharepoint.com/sites/legal-stg";
            principal_app2.IsActive = true;
            principal_app2.ErrorEmailTo = "ddutta@abc.com";
            principal_app2.ErrorEmailCC = "jamali@abc.com";

            #region App2 List1
            principal_app2.AppLists.Add(new AppListDetails()
            {
                ApplicationName = "Provide A Gift",
                ListName = "Provide_Gift",
                IsPrimary = true,
                IsActive = true,
                AppTracer = new AppLog()
                {
                    IdChecked = { 40, 50, 60 },
                    //ErrorsOccurred = { 55 },
                    ErrorsOccurred = { },

                    InspectingItems = new List<RequestItem>()
                    {
                        new RequestItem()
                        {
                            ErrorType = "",
                            ItemId = 40,
                            PermissionUrl = "",
                            Title = "Title 40"
                        },
                        new RequestItem()
                        {
                            ErrorType = "",
                            ItemId = 55,
                            PermissionUrl = "",
                            Title = "Title 55"
                        },
                        new RequestItem()
                        {
                            ErrorType = "",
                            ItemId = 50,
                            PermissionUrl = "",
                            Title = "Title 50"
                        },
                        new RequestItem()
                        {
                            ErrorType = "",
                            ItemId = 60,
                            PermissionUrl = "",
                            Title = "Title 60"
                        }
                    }
                },
                SecurityCheck = new AppPermissionDetails()
                {
                    RuleName = "Unique Permisssion",
                    PermissionType = "Unique",
                    PermissionLevel = Constants.AppPermissionDetailsColumns.PermissionLevelValues.ITEM_LEVEL,
                    PermissionScope = Constants.AppPermissionDetailsColumns.PermissionScopeValues.UNIQUE,
                }
            });
            #endregion

            #region App2 List2
            principal_app2.AppLists.Add(new AppListDetails()
            {
                ApplicationName = "Provide A Gift",
                ListName = "ProvideGiftNewAttachment",
                IsPrimary = false,
                IsActive = true,
                AppTracer = new AppLog()
                {
                    IdChecked = { 5, 6, 7, 8 },
                    ErrorsOccurred = { },

                    InspectingItems = new List<RequestItem>()
                    {
                        new RequestItem()
                        {
                            ErrorType = "",
                            ItemId = 5,
                            PermissionUrl = "",
                            Title = "Title 5"
                        },
                        new RequestItem()
                        {
                            ErrorType = "",
                            ItemId = 6,
                            PermissionUrl = "",
                            Title = "Title 6"
                        },
                        new RequestItem()
                        {
                            ErrorType = "",
                            ItemId = 7,
                            PermissionUrl = "",
                            Title = "Title 7"
                        },
                        new RequestItem()
                        {
                            ErrorType = "",
                            ItemId = 8,
                            PermissionUrl = "",
                            Title = "Title 8"
                        }
                    }
                },
                SecurityCheck = new AppPermissionDetails()
                {
                    RuleName = "Unique Permisssion",
                    PermissionType = "Unique",
                    PermissionLevel = Constants.AppPermissionDetailsColumns.PermissionLevelValues.ITEM_LEVEL,
                    PermissionScope = Constants.AppPermissionDetailsColumns.PermissionScopeValues.UNIQUE,
                }
            });
            #endregion

            #endregion

            apps.Add(principal_app1);
            apps.Add(principal_app2);

            return apps;
        }        
    }
}