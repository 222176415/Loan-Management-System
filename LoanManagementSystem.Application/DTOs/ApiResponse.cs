namespace LoanManagementSystem.Application.DTOs;

public record ApiResponse<T>(bool Success, T? Data, string? Message);