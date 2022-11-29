using System;

namespace PermissionCheckerDaemon.Exceptions
{
    class ApplicationListDetailsException : Exception
    {
        public ApplicationListDetailsException() : base() { }
        public ApplicationListDetailsException(string message) : base(message) { }
        public ApplicationListDetailsException(string message, Exception inner) : base(message, inner) { }
    }
}
