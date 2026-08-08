namespace LoanManagementSystem.Application.DTOs;



public record CreateLoanRequest(
    // Core Financial Metrics
    decimal PrincipalAmount,
    DateTime DueDate,

    // Client Onboarding Footprint Identity
    string Email,
    string FirstName,
    string Surname,
    string? PhoneNumber = null,
    string? Address = null,

    // Optional legacy reference property for localized client matching
    int? ClientId = null
);

public record LoanResponse(
    int Id,
    decimal PrincipalAmount,
    decimal InterestRate,
    decimal TotalAmountDue,
    string Status,
    DateTime DueDate,
    ClientResponse Client 
);
public record ClientResponse(
    int Id,
    string FirstName,
    string Surname,
    string Email,
    string PhoneNumber,
    string Address
);


public record CreateClientRequest(
    string FirstName, 
    string Surname, 
    string Email, 
    string PhoneNumber, 
    string Address
);