namespace LoanManagementSystem.Domain.Entities;
using LoanManagementSystem.Domain.Enums;
public class Loan
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public int OrganizationId { get; set; }
    public decimal PrincipalAmount { get; set; }
    public decimal InterestRate { get; set; }
    public decimal VatRate { get; set; }
    public decimal TotalAmountDue { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public LoanStatus Status { get; set; } = LoanStatus.Active;
    public bool IsReminded { get; set; }
    
    public bool IsDeleted { get; set; } = false;
    public int? DeletedById { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation
    public Client Client { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}