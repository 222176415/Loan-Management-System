namespace LoanManagementSystem.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;
    public int? DeletedById { get; set; }
    public DateTime? DeletedAt { get; set; }
    // Navigation
    public Organization Organization { get; set; } = null!;
    public Role Role { get; set; } = null!;
}