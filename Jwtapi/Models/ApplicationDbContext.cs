using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Jwtapi.Models
{
    public class ApplicationDbContext:IdentityDbContext<ApplicationUser> 
    {
        public ApplicationDbContext( DbContextOptions<ApplicationDbContext> options ):base(options)
        {

        }
    }
}
