using static System.Console;

namespace PermissionCheckerDaemon.Entities
{
    static class ErrorInfo
    {
        private static string _primaryErrorMessage;
        private static string _secondaryErrorMessage;

        public static string PrimaryErrorMessage
        {
            get { return _primaryErrorMessage; }
            set
            {
                _primaryErrorMessage = value;
                WriteLine(_primaryErrorMessage);
            }
        }

        public static string SecondaryErrorMessage
        {
            get { return _secondaryErrorMessage; }
            set
            {
                _secondaryErrorMessage = value;
                WriteLine(_secondaryErrorMessage);
            }
        }
    }
}