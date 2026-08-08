namespace LoanManagementSystem.Domain.Entities;


public class Client
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool IsDeleted { get; set; } = false;
    public int? DeletedById { get; set; }
    public DateTime? DeletedAt { get; set; }
    // Navigation
    public Organization Organization { get; set; } = null!;
}