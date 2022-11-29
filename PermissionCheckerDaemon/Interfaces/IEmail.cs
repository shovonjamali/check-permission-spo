using PermissionCheckerDaemon.Entities;

namespace PermissionCheckerDaemon.Interfaces
{
    interface IEmail
    {
        void SendEmail(EmailObject email_object);
    }
}
