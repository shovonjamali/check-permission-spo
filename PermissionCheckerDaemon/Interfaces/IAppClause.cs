using Microsoft.SharePoint.Client;
using PermissionCheckerDaemon.Entities;
using System.Collections.Generic;
using System.Linq;

namespace PermissionCheckerDaemon.Interfaces
{
    interface IAppClause
    {
        IEnumerable<IGrouping<string, AppDetails>> CreateAppSecurityMaster();
    }
}
