using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using LoanManagementSystem.Domain.Entities;
using LoanManagementSystem.Application.Interfaces;

namespace LoanManagementSystem.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    private readonly ICurrentTenantService _tenantService;


    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options, 
        ICurrentTenantService tenantService) : base(options) 
    {
        _tenantService = tenantService;
    }

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<UserLoginLog> UserLoginLogs => Set<UserLoginLog>();
    public DbSet<UserSecurityLog> UserSecurityLogs { get; set; }
    
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            optionsBuilder.UseSqlServer(config.GetConnectionString("DbConnection"));
            optionsBuilder.EnableSensitiveDataLogging();
        }
    }


    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("dbo");
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        //  Multi-Tenancy: Global Query Filters & Deleted Entries
        modelBuilder.Entity<User>()
            .HasQueryFilter(u => u.OrganizationId == _tenantService.OrganizationId);
        modelBuilder.Entity<Loan>()
            .HasQueryFilter(x => x.OrganizationId == _tenantService.OrganizationId && !x.IsDeleted);

        modelBuilder.Entity<Client>()
           .HasQueryFilter(x => x.OrganizationId == _tenantService.OrganizationId && !x.IsDeleted);

        
        var decimalProperties = modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?));

        foreach (var property in decimalProperties)
        {
            property.SetPrecision(18);
            property.SetScale(2);
        }
        foreach (var foreignKey in modelBuilder.Model.GetEntityTypes()
                     .SelectMany(e => e.GetForeignKeys()))
        {
            foreignKey.DeleteBehavior = DeleteBehavior.NoAction;
        }

        base.OnModelCreating(modelBuilder);
    }

}
