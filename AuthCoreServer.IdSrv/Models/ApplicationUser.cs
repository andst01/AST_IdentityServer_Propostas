using Microsoft.AspNetCore.Identity;

namespace AuthCoreServer.IdSrv.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public string Nome { get; set; }
    }
}
