using System.Collections.Generic;

namespace PermissionCheckerDaemon.Entities
{
    class AppLog
    {
        public List<int> IdChecked { get; set; } = new List<int>();
        public List<int> ErrorsOccurred { get; set; } = new List<int>();
        public List<RequestItem> InspectingItems { get; set; } = new List<RequestItem>();
    }
}
