namespace PermissionCheckerDaemon.Configuration
{
    public class Constants
    {
        #region LISTS
        public struct AppLists
        {
            public const string APPLICATION_DETAILS = "SST-ApplicationDetails";
            public const string APPLICATION_LIST_DETAILS = "SST-ApplicationListDetails";
            public const string APPLICATION_PERMISSION_DETAILS = "SST-ApplicationPermissionDetails";
            public const string APPLICATION_LOG = "SST-Log";
            public const string APPLICATION_ERROR_LOG = "SST-Error-Log";
            public const string APPLICATION_PERMISSION_ORDER = "SST-PermissionChronology";
        }
        #endregion

        #region LIST COLUMNS
        public struct AppDetailsColumns
        {
            public const string APPLICATION_NAME = "Title";
            public const string ACTIVE = "Active";
            public const string SITE_URL = "Site_x0020_URL";
            public const string ERROR_EMAIL_TO = "Email_x0020_To";
            public const string ERROR_EMAIL_CC = "Email_x0020_CC";
            public const string CLIENT_ID = "ClientID";
            public const string CLIENT_SECRET = "ClientSecret";
        }

        public struct AppListDetailsColumns
        {
            public const string APPLICATION_NAME = "Application_x0020_Name";
            public const string LIST_NAME = "List_x002f_Library";
            public const string SECURITY_CHECK = "Security_x0020_Check";
            public const string PRIMARY = "Primary";
            public const string ACTIVE = "Active";
        }

        public struct AppPermissionDetailsColumns
        {
            public const string RULE_NAME = "Title";
            public const string ACCOUNTS_TO_BE_VERIFIED = "Account_x0020_to_x0020_be_x0020_";
            public const string PERMISSION_TYPE = "Permission_x0020_Type";
            public const string PERMISSION_LEVEL = "Permission_x0020_Level";
            public const string PERMISSIONSCOPE = "PermissionScope";
            public struct PermissionScopeValues
            {
                public const string INHERITED = "Inherited";
                public const string UNIQUE = "Unique";
            }

            public struct PermissionLevelValues
            {
                public const string ITEM_LEVEL= "Item Level";
                public const string LIST_LEVEL = "List Level";
                public const string SITE_LEVEL = "Site Level";
            }

            public struct PermissionTypeValues
            {
                public const string NO_ACCESS = "None";
                public const string READ = "Read";
                public const string CONTRIBUTE = "Contribute";
                public const string EDIT = "Edit";
                public const string FULL_CONTROL = "Full Control";
            }
        }

        public struct LogColumns
        {
            public const string APPLICATION_NAME = "Title";
            public const string LIST_NAME = "List_x002f_Library";
            public const string ID_CHECKED = "Ids_x0020_checked";
            public const string ERRORS_OCCURED = "Errors_x0020_Occurred";
        }

        public struct AppErrorLogColumns
        {
            public const string TITLE = "Title";
            public const string MESSAGE = "Message";            
        }
        #endregion

        #region Error Log Details
        public struct ErrorLogDetails
        {
            public struct SiteErrorTypeValue
            {
                public const string SCOPE_ERROR = "Site Scope Error";
                public const string ROLE_ERROR = "Site Role Error";
            }
            public struct ListErrorTypeValue
            {
                public const string SCOPE_ERROR = "List Scope Error";
                public const string ROLE_ERROR = "List Role Error";
            }

            public struct ItemErrorTypeValue
            {
                public const string SCOPE_ERROR = "Item Scope Error";
                public const string ROLE_ERROR = "Item Role Error";
            }
        }
        #endregion

        #region Exception Message 
        public static class CustomExceptionMessages
        {
            public static string PERMISSION_DETAILS = $"Exception occured while fetching {AppLists.APPLICATION_PERMISSION_DETAILS}";
            public static string LIST_DETAILS = $"Exception occured while fetching {AppLists.APPLICATION_LIST_DETAILS}";
            public static string APPLICATION_DETAILS = $"Exception occured while fetching {AppLists.APPLICATION_DETAILS}";
            public static string EMAIL_OBJECT = $"Exception occured while fetching {AppLists.APPLICATION_DETAILS}";
        }
        #endregion

        public struct SecurityGroups
        {
            public const string EVERYONE = "Everyone";
            public const string EVERYONE_EXCEPT_EXTERNAL_USERS = "Everyone except external users";
        }

        public struct AppPermissionOrderColumns
        {
            public const string ROLE = "Title";
            public const string Order_NO = "OrderNo";
        }
    }
}