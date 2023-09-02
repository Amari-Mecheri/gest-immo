using Gest_Immo_API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Gest_Immo_API.Data
{
    public class Context : IdentityDbContext<User>
    {
        public Context(DbContextOptions<Context> options):base(options)
        {
            
        }
    }
}
