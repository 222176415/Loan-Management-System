namespace LoanManagementSystem.Application.DTOs;

public record LoginRequest(string Email, string Password);
public record AuthResponse(
    string Token, 
    string Email, 
    int OrganizationId, 
    string OrganizationName, 
    string Role, 
    string? FullNames = null
);
public record CreateUserRequest(string FullName, string Email, string Password, int RoleId);
public record UpdateUserRequest(string FullName, int RoleId, bool IsActive);
public record UserResponse(int Id, string FullName, string Email, int RoleId, string RoleName, bool IsActive);
public record ResetPasswordRequest(
    string Email, 
    string NewPassword
);

public record CreateOrganizationRequest(string Name, string Email, decimal VatRate, decimal DefaultInterestRate);
public record UpdateOrganizationRequest(string Name, decimal VatRate, decimal DefaultInterestRate);
public record OrganizationResponse(int Id, string Name, string Email, decimal VatRate, decimal DefaultInterestRate);
public record RoleResponse(int Id, string Name);


public record CreatePaymentRequest(int LoanId, decimal AmountPaid, string PaymentMethod);
public record PaymentResponse(int Id, int LoanId, decimal AmountPaid, DateTime PaymentDate, string PaymentMethod);
