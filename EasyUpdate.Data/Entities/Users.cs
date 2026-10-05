using Microsoft.AspNetCore.Identity;

namespace EasyUpdate.Data.Entities
{
    public class Users : IdentityUser
    {
        public string FullName { get; set; }
    }
}
