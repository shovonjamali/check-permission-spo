using PermissionCheckerDaemon.Entities;
using System.Collections.Generic;
using System.Linq;

namespace PermissionCheckerDaemon.Interfaces
{
    interface IPermissionChecker
    {
        List<AppDetails> CheckPermission(IEnumerable<IGrouping<string, AppDetails>> grouped_by_site);
    }
}
