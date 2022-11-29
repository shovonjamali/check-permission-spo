using System.Collections.Generic;

namespace PermissionCheckerDaemon.Entities
{
    class AppPermissionDetails
    {
        public string RuleName { get; set; }
        public List<string> AccountsToBeVerified { get; set; }
        public string PermissionType { get; set; }
        public string PermissionLevel { get; set; }
        public string PermissionScope { get; set; }
    }
}
