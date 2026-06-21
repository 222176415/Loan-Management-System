namespace LoanManagementSystem.Application.DTOs;



public record CreateLoanRequest(
    int ClientId,
    decimal PrincipalAmount,
    DateTime DueDate
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
    string LastName,
    string Email,
    string PhoneNumber,
    string Address
);


public record CreateClientRequest(
    string FirstName, 
    string LastName, 
    string Email, 
    string PhoneNumber, 
    string Address
);