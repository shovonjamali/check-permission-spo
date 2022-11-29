namespace PermissionCheckerDaemon.Entities
{
    class EmailAddress
    {
        public EmailAddress() { }

        public EmailAddress(string toAddress, string ccAddress)
        {
            TOAddress = toAddress;
            CCAddress = ccAddress;
        }

        public string TOAddress { get; set; }
        public string CCAddress { get; set; }
    }
}
