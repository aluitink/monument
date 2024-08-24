namespace monument.api.client.Models
{
    public class ClientPrincipal
    {
        public string? IdentityProvider { get; set; }
        public string? UserId { get; set; }
        public string? UserDetails { get; set; }
        public List<string> UserRoles { get; set; } = new List<string>();
        public override string ToString()
        {
            return $"IdentityProvider: {IdentityProvider}, UserId: {UserId}, UserDetails: {UserDetails}, UserRoles: [{string.Join(',', UserRoles)}]";
        }
    }
}
