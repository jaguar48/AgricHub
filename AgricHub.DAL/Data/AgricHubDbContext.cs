
using Microsoft.EntityFrameworkCore;

namespace AgricHub.DAL.Data
{
    public class AgricHubDbContext : IdentityDbContext<User>
    {
        
            public AgricHubDbContext(DbContextOptions<AgricHubDbContext> options)
                : base(options)
            {
            }
        
    }
}
