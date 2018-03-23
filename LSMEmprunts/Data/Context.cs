using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LSMEmprunts.Data
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options)
            :base(options)
        { }

        public DbSet<User> Users { get; set; }
        public DbSet<Gear> Gears { get; set; }
        public DbSet<Borrowing> Borrowings { get; set; }
        public DbSet<GearFailure> Failures { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new UserMapping());
            modelBuilder.ApplyConfiguration(new GearMapping());
            modelBuilder.ApplyConfiguration(new BorrowingMapping());
            modelBuilder.ApplyConfiguration(new GearFailureMapping());
        }
    }

    public class ContextesignTimeFactory : IDesignTimeDbContextFactory<Context>
    {
        public Context CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<Context>();
            optionsBuilder.UseSqlite("Data Source=DesignTime.db");

            return new Context(optionsBuilder.Options);
        }
    }
}
