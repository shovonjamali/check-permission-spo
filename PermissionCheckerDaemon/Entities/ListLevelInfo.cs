namespace PermissionCheckerDaemon.Entities
{
    /// <summary>
    /// This Class has been used to manage the List and Site Level Check
    /// This was nammed in the first version when site level permission check has not been implemented
    /// Later, this has been used for the site level error info to store too.
    /// </summary>
    class ListLevelInfo
    {
        public bool HasError { get; set; }
        public string ErrorType { get; set; }
        public string PermissionUrl { get; set; }
    }
}
