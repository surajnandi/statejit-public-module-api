namespace sjam.Dal.Enum
{
    public enum AppEnvironment
    {
        DEV,
        UAT,
        PROD
    }
    public enum APIResponseStatus
    {
        Success = 1,
        Warning = 2,
        Error = 3,
        FyEndBlocked = 4,
        FeatureBlock = 99
    }
    public static class RoleEnum
    {
        public const string APPROVER = "approver";
        public const string OPERATOR = "operator";
        public const string ADMIN = "admin";
        public const string STATEJIT_ADMIN = "statejit_admin";
        public const string STATEJIT_AGENCY_ADMIN = "statejit_agency_admin";
        public const string STATEJIT_SYSTEM_ADMIN = "statejit_system_admin";
    }
}
