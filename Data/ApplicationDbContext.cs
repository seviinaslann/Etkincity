using Etkincity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Etkincity.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Event> Events { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<UserEventView> UserEventViews { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Event>().Property(e => e.Price).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Reservation>().Property(r => r.TotalPrice).HasColumnType("decimal(18,2)");
    }
}
