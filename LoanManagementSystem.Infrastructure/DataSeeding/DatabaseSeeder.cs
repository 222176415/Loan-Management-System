using LoanManagementSystem.Application.Security;

namespace LoanManagementSystem.Infrastructure.Persistence;
using LoanManagementSystem.Domain.Entities;
public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // 1. Seed Roles if empty
        if (!context.Roles.Any())
        {
            context.Roles.AddRange(
                new Role { Name = "Admin" },
                new Role { Name = "LoanOfficer" }
            );
            await context.SaveChangesAsync();
        }

        // 2. Seed a Default Organization if empty
        if (!context.Organizations.Any())
        {
            var org = new Organization
            {
                Name = "Main Branch",
                Email = "admin@mainbranch.com",
                VatRate = 15.0m,
                DefaultInterestRate = 10.0m
            };
            context.Organizations.Add(org);
            await context.SaveChangesAsync();
            
            // 3. Seed a Default Admin User for this Org
            if (!context.Users.Any())
            {
                context.Users.Add(new User
                {
                    FullName = "System Admin",
                    Email = "ntimanethemba27@gmail.com",
                    PasswordHash = PasswordHasher.HashPassword("themba-dev"),
                    OrganizationId = org.Id,
                    RoleId = context.Roles.First(r => r.Name == "Admin").Id
                });
                await context.SaveChangesAsync();
            }
        }
    }
}