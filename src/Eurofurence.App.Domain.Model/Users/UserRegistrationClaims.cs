namespace Eurofurence.App.Domain.Model.Users
{
    public static class UserRegistrationClaims
    {
        public static string Id = "RegSysId";
        public static string StatusPrefix = "RegSysStatus";
        public static string Status(string id)
        {
            return $"{StatusPrefix}:{id}";
        }
    }
}