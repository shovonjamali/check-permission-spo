namespace PermissionCheckerDaemon.Entities
{
    class RequestItem
    {
        public int ItemId { get; set; }
        public string Title { get; set; }
        public string ItemUrl { get; set; }
        public string PermissionUrl { get; set; }
        public string ErrorType { get; set; } = string.Empty;
    }
}
