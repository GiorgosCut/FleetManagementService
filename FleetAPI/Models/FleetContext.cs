using Microsoft.EntityFrameworkCore;
using API.Models;

namespace API.Context
{
    public class FleetContext : DbContext
    {
        public FleetContext(DbContextOptions<FleetContext> options) : base(options) { }

        public  DbSet<Vessel>  Vessels { get; set; }
        public  DbSet<Container> Containers { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
            modelBuilder.Entity<Vessel>().HasKey(x => x.Id);
            modelBuilder.Entity<Container>().HasKey(x => x.Id);

            modelBuilder.Entity<Container>()
                .HasOne(x => x.Vessel)
                .WithMany(x => x.Containers);

                //I was getting code 500 errors when trying to assign foreign key to containers.
                //Could be the problem that its an in-memory db
                //.HasForeignKey(x => x.Vessel.Id);
        }
    }
}
