namespace LoanManagementSystem.Application.DTOs;


    public record UpdateClientRequest(
        string FirstName, 
        string Surname, 
        string Email, 
        string PhoneNumber, 
        string Address
    );
