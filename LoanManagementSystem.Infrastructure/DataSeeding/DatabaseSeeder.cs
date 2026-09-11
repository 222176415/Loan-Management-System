using LoanManagementSystem.Application.Security;
using LoanManagementSystem.Domain.Entities;

namespace LoanManagementSystem.Infrastructure.Persistence;

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

        var adminRoleId = context.Roles.First(r => r.Name == "Admin").Id;

        // 2. Seed Main Branch Organization & User if empty
        if (!context.Organizations.Any(o => o.Name == "Main Branch"))
        {
            var mainOrg = new Organization
            {
                Name = "Main Branch",
                Email = "admin@mainbranch.com",
                VatRate = 15.0m,
                DefaultInterestRate = 10.0m
            };
            context.Organizations.Add(mainOrg);
            await context.SaveChangesAsync();

            if (!context.Users.Any(u => u.Email == "ntimanethemba27@gmail.com"))
            {
                context.Users.Add(new User
                {
                    FullName = "System Admin",
                    Email = "ntimanethemba27@gmail.com",
                    PasswordHash = PasswordHasher.HashPassword("themba-dev"),
                    OrganizationId = mainOrg.Id,
                    RoleId = adminRoleId
                });
                await context.SaveChangesAsync();
            }
        }

        // 3. Seed Singular Systems Organization & User if empty
        if (!context.Organizations.Any(o => o.Name == "Singular Systems"))
        {
            var singularOrg = new Organization
            {
                Name = "Singular Systems",
                Email = "info@singular.co.za",
                VatRate = 15.0m,
                DefaultInterestRate = 12.5m
            };
            context.Organizations.Add(singularOrg);
            await context.SaveChangesAsync();

            if (!context.Users.Any(u => u.Email == "mabena@singular.co.za"))
            {
                context.Users.Add(new User
                {
                    FullName = "Mthandazo Ben Mabena",
                    Email = "mabena@singular.co.za",
                    PasswordHash = PasswordHasher.HashPassword("singular"),
                    OrganizationId = singularOrg.Id,
                    RoleId = adminRoleId
                });
                await context.SaveChangesAsync();
            }
        }
    }
}