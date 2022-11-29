using System;

namespace PermissionCheckerDaemon.Exceptions
{
    class ApplicationPermissionDetailsException : Exception
    {
        public ApplicationPermissionDetailsException() : base() { }
        public ApplicationPermissionDetailsException(string message) : base(message) { }
        public ApplicationPermissionDetailsException(string message, Exception inner) : base(message, inner) { }
    }
}
