namespace LoanManagementSystem.Domain.Entities;

public class Payment
{
    public int Id { get; set; }
    public int LoanId { get; set; }
    public decimal AmountPaid { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public string PaymentMethod { get; set; } = "Bank Transfer / EFT";

    // Navigation
    public Loan Loan { get; set; } = null!;
}