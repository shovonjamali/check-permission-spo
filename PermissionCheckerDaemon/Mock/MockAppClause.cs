using PermissionCheckerDaemon.Interfaces;
using System.Collections.Generic;
using System.Linq;
using PermissionCheckerDaemon.Entities;

namespace PermissionCheckerDaemon.Mock
{
    class MockAppClause : IAppClause
    {
        public IEnumerable<IGrouping<string, AppDetails>> CreateAppSecurityMaster()
        {
            return new List<AppDetails>().GroupBy(m => m.SiteURL);
        }
    }
}
