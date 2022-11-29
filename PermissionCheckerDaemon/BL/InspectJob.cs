using System;
using PermissionCheckerDaemon.Entities;
using PermissionCheckerDaemon.Interfaces;
using PermissionCheckerDaemon.Services;
using Unity;

namespace PermissionCheckerDaemon.BL
{
    class InspectJob
    {
        private IAppClause _appClause = null;
        private IPermissionChecker _permissionDeterminant = null;
        private IAppLogger _appLogger = null;
        private IEmail _emailHandler = null;
        
        private Helper _helper = null;
        private EmailManager _emailManager = null;

        [InjectionConstructor]
        public InspectJob(IAppClause appClause, IPermissionChecker permissionChecker, IAppLogger appLogger, IEmail emailHandler) 
        {
            _appClause = appClause;
            _permissionDeterminant = permissionChecker;
            _appLogger = appLogger;
            _emailHandler = emailHandler;

            _emailManager = new EmailManager();
            _helper = new Helper();
        }

        public void Run()
        {
            try
            {
                var master_security_collection = _appClause.CreateAppSecurityMaster();
                //_helper.DumpResult(master_security_collection);

                // Iterate Application details and perform operation on each application
                var checked_apps = _permissionDeterminant.CheckPermission(master_security_collection);
                //var checked_apps = _permissionDeterminant.CheckPermissionParallel(master_security_collection);
                
                // Log result for reference
                _appLogger.LogJob(checked_apps);

                #region Email Notification / JIRA ticket
                // Create email object to send
                EmailObject emailObject = _emailManager.GetEmailObject(checked_apps);

                // Send email
                if(_helper.EmailSendingStatus(emailObject) == true)
                    _emailHandler.SendEmail(emailObject);
                #endregion
            }
            catch (Exception ex)
            {
                ErrorInfo.PrimaryErrorMessage = $"Exception Message: {ex.Message}\nStack Trace: {ex.StackTrace}.";
            }
            finally
            {
                new ErrorLog().WriteLog();
            }          
        }
    }
}