namespace AuthApi.Models
{
    public class UpdateRolesRequest
    {
        public List<UserRole> Roles { get; set; } = null!;
    }
}
