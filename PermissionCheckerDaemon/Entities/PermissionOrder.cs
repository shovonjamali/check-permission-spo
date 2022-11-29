using System;
using System.Collections.Generic;
using Microsoft.SharePoint.Client;
using PermissionCheckerDaemon.Services;
using PermissionCheckerDaemon.Configuration;

namespace PermissionCheckerDaemon.Entities
{
    class PermissionOrder
    {
        public Dictionary<string, int> DicPermissionOrder{get; set;} = new Dictionary<string, int>();

        private General _svcGeneral;
        public PermissionOrder() { }
        public PermissionOrder(ClientContext context)
        {
            _svcGeneral = new General();
            DicPermissionOrder = getPermissionOrder(context);
        }

        private Dictionary<string, int> getPermissionOrder(ClientContext context)
        {
            Dictionary<string, int> dicPermissionOrder = new Dictionary<string, int>();
            List<ListItem> items = _svcGeneral.GetAllItems(context, listName: Constants.AppLists.APPLICATION_PERMISSION_ORDER);
            foreach(ListItem item in items)
            {
                string role = Convert.ToString(item[Constants.AppPermissionOrderColumns.ROLE]);
                int orderNo = Convert.ToInt32(item[Constants.AppPermissionOrderColumns.Order_NO]);
                dicPermissionOrder.Add(role, orderNo);
            }
            return dicPermissionOrder;
        }

    }
}
