namespace PermissionCheckerDaemon.Entities
{
    class EmailObject
    {
        public EmailAddress Recipients { get; set; }
        public EmailContent Content { get; set; }
        public bool FaultyEmail { get; set; }

        public EmailObject() { }

        public EmailObject(EmailAddress recipients, EmailContent content, bool faultyEmail)
        {
            Recipients = recipients;
            Content = content;
            FaultyEmail = faultyEmail;
        }
    }
}
