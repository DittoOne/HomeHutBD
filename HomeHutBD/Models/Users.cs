using Microsoft.AspNetCore.Identity;

namespace HomeHutBD.Models
{
    public class Users : IdentityUser
    {
        public string Fullname { get; set; }
    }
}
