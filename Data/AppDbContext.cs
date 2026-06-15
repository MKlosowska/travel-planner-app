using Microsoft.EntityFrameworkCore;
using TravelPlannerApp.Models;

namespace TravelPlannerApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<PasswordResetCode> PasswordResetCodes => Set<PasswordResetCode>();
    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<TripUser> TripUsers => Set<TripUser>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<DocumentFile> Documents => Set<DocumentFile>();
    public DbSet<PhotoFile> Photos => Set<PhotoFile>();
    public DbSet<LocationPoint> Locations => Set<LocationPoint>();
    public DbSet<PackingList> PackingLists => Set<PackingList>();
    public DbSet<PackingItem> PackingItems => Set<PackingItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<TripUser>().HasIndex(tu => new { tu.TripId, tu.UserId }).IsUnique();
        modelBuilder.Entity<Trip>().HasQueryFilter(t => !t.IsDeleted);
        modelBuilder.Entity<Expense>().Property(e => e.Amount).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Trip>().Property(t => t.BudgetAmount).HasColumnType("decimal(18,2)");
    }
}
