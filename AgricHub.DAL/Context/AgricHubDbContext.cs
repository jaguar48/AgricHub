

using AgricHub.DAL.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AgricHub.DAL.Context
{
    public class AgricHubDbContext : IdentityDbContext<ApplicationUser>
    {
        
            public AgricHubDbContext(DbContextOptions<AgricHubDbContext> options)
                : base(options)
            {
            }
        
    }
}
