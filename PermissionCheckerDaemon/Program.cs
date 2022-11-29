using static System.Console;
using PermissionCheckerDaemon.BL;
using Unity;
using PermissionCheckerDaemon.Interfaces;
using PermissionCheckerDaemon.Services;
using PermissionCheckerDaemon.Mock;

namespace PermissionCheckerDaemon
{
    class Program
    {
        static void Main(string[] args)
        {
            WriteLine("Job started");

            // Instantiating Unity IoC container
            var containter = new UnityContainer();

            // Registering dependencies
            containter.RegisterType<IAppClause, AppClauseList>();
            containter.RegisterType<IPermissionChecker, PermissionDeterminant>();
            //containter.RegisterType<IPermissionChecker, MockPermissionDeterminant>();   // To use mock data
            containter.RegisterType<IAppLogger, LogSPList>();
            //containter.RegisterType<IEmail, EmailSPUtility>();    // Send email using sp utility
            containter.RegisterType<IEmail, EmailEWS>();            // send email using EWS [required for creating JIRA ticket and to specify from address]

            var inspectJob = containter.Resolve<InspectJob>();

            inspectJob.Run();

            WriteLine("\nJob completed\n");
            //ReadLine();
        }
    }
}