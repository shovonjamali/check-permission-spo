namespace PermissionCheckerDaemon.Entities
{
    public class MemberDetails
    {
        public string PrincipleType { get; set; }
        public string Permission { get; set; }

        public MemberDetails() { }
        public MemberDetails(string principleType, string permission)
        {
            PrincipleType = principleType;
            Permission = permission;
        }
        internal void UpdatePermission(string permission)
        {
            Permission += permission;
        }
    }
}
