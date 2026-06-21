namespace LoanManagementSystem.Domain.Entities;

public class Organization
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal VatRate { get; set; }
    public decimal DefaultInterestRate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public int? DeletedById { get; set; }
    public DateTime? DeletedAt { get; set; }
    // Navigation
    public ICollection<Client> Clients { get; set; } = new List<Client>();
}