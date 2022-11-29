using System;

namespace PermissionCheckerDaemon.Exceptions
{
    class ApplicationDetailsException : Exception
    {
        public ApplicationDetailsException() : base() { }
        public ApplicationDetailsException(string message) : base(message) { }
        public ApplicationDetailsException(string message, Exception inner) : base(message, inner) { }
    }
}
