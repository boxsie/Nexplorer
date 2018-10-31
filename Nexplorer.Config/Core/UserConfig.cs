namespace Nexplorer.Config.Core
{
    public class UserConfig
    {
        public const string SuperUserPolicy = "SuperUserPolicy";
        public const string AdminUserPolicy = "AdminUserPolicy";
        public const string EditorUserPolicy = "EditorUserPolicy";
        public const string UserPolicy = "UserPolicy";

        public UserRole[] UserRoles { get; set; }
    }
}