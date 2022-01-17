namespace Laboremus_AuthorizationService.Core.Helpers.Logger
{
    public enum Status
    {
        Deleted = 0,
        Active = 1,
        Locked = 2,
        Modified = 3,
        Hidden = 4
    }

    /// <summary>
    /// Method
    /// </summary>
    public enum Method
    {
        /// <summary>
        /// Add
        /// </summary>
        Add = 1,

        /// <summary>
        /// Update
        /// </summary>
        Update = 2,

        /// <summary>
        /// Delete
        /// </summary>
        Delete = 3
    }   

    /// <summary>
    /// Service
    /// </summary>  
    public enum ClientService
    {
        /// <summary>
        /// CRM Service
        /// </summary>
        CrmService = 1,

        /// <summary>
        /// Auth Service
        /// </summary>
        AuthService = 2,

        /// <summary>
        /// Ledger Service
        /// </summary>
        LedgerService = 3,  

        /// <summary>
        /// Sync Service
        /// </summary>
        SyncService = 3,

        /// <summary>
        /// Back Office
        /// </summary>
        BackOffice = 4,

        /// <summary>
        /// Logging Service
        /// </summary>
        LoggingService = 5,

        /// <summary>
        /// Notification Service
        /// </summary>
        NotificationService = 6
    }
}
