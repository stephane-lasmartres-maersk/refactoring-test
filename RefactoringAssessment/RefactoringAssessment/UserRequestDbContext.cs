using Microsoft.EntityFrameworkCore;

namespace RefactoringAssessment
{
    internal class UserRequestDbContext : DbContext
    {
        public virtual DbSet<UserRequest> UserRequests { get; set; }

        public UserRequestDbContext(DbContextOptions<UserRequestDbContext> options)
           : base(options)
        { }
    }
}
