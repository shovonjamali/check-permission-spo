using PermissionCheckerDaemon.Entities;
using System.Collections.Generic;

namespace PermissionCheckerDaemon.Interfaces
{
    interface IAppLogger
    {
        void LogJob(List<AppDetails> apps);
    }
}
