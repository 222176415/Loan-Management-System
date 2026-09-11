using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LoanManagementSystem.Application.DTOs;
using LoanManagementSystem.Application.Security;
using LoanManagementSystem.Application.Interfaces;
using LoanManagementSystem.Domain.Entities;
using LoanManagementSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LoanManagementSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    IConfiguration config, 
    ApplicationDbContext context, 
    IAuditService auditService) : ControllerBase
{
    [HttpPost("login")]
   public async Task<IActionResult> Login([FromBody] LoginRequest request)
{
    try
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        var userAgent = Request.Headers["User-Agent"].ToString() ?? "Unknown Client";
        
 
        var user = await context.Users
            .IgnoreQueryFilters() 
            .Include(u => u.Role) // Eagerly load Role entity
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            await auditService.LogLoginAsync(request.Email, false, "Invalid credentials");
            var failureLog = new UserLoginLog
            {
                UserEmail = request.Email ?? "unknown_operator",
                IpAddress = ipAddress,
                UserAgent = userAgent,
                IsSuccess = false,
                FailureReason = user == null ? "Account footprint does not exist" : "Invalid credentials supplied",
                Timestamp = DateTime.UtcNow
            };

            context.UserLoginLogs.Add(failureLog);
            await context.SaveChangesAsync();
            return Unauthorized(new ApiResponse<string>(false, null, "Invalid email or password."));
        }

        if (!user.IsActive)
        {
            await auditService.LogLoginAsync(request.Email, false, "Account deactivated");
            var suspendedLog = new UserLoginLog
            {
                UserEmail = user.Email,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                IsSuccess = false,
                FailureReason = "Account suspended Login Attempt",
                Timestamp = DateTime.UtcNow
            };

            context.UserLoginLogs.Add(suspendedLog);
            await context.SaveChangesAsync();
            return BadRequest(new ApiResponse<string>(false, null, "Your account is deactivated."));
        }

        var token = GenerateJwtToken(user);
        
        var orgName = await context.Organizations
            .IgnoreQueryFilters() 
            .Where(u => u.Id == user.OrganizationId)
            .Select(u => u.Name)
            .FirstOrDefaultAsync() ?? "Unknown Organization";

        // Resolve Role Name (Fall back to direct lookup if user.Role navigation property is not configured)
        string roleName = user.Role?.Name;
        if (string.IsNullOrEmpty(roleName))
        {
            roleName = await context.Roles
                .Where(r => r.Id == user.RoleId)
                .Select(r => r.Name)
                .FirstOrDefaultAsync() ?? "User";
        }

        await auditService.LogLoginAsync(request.Email, true);

  
        var response = new AuthResponse(
            Token: token,
            Email: user.Email,
            OrganizationId: user.OrganizationId,
            OrganizationName: orgName,
            Role: roleName,
            FullNames: user.FullName 
        );

        return Ok(new ApiResponse<AuthResponse>(true, response, "Login successful"));
    }
    catch (Exception ex)
    {
        return StatusCode(500, new ApiResponse<string>(false, null, $"Login Error: {ex.Message}"));
    }
}
    private string GenerateJwtToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("OrganizationId", user.OrganizationId.ToString()),
            new Claim("RoleId", user.RoleId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(config["Jwt:DurationInMinutes"] ?? "60")),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    [HttpPost("reset-password")]
public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
{
    try
    {
        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
        {
            await auditService.LogSecurityActionAsync(request.Email, false, "Password criteria failed");
            return BadRequest(new ApiResponse<string>(false, null, "Password must be at least 6 characters long."));
        }
        var user = await context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            await auditService.LogSecurityActionAsync(request.Email, false, "User not discovered");
            return NotFound(new ApiResponse<string>(false, null, "User account with this email does not exist."));
        }

        if (!user.IsActive)
        {
            await auditService.LogSecurityActionAsync(request.Email, false, "Account deactivated");
            return BadRequest(new ApiResponse<string>(false, null, "Cannot reset password for a deactivated account."));
        }
        
        user.PasswordHash = PasswordHasher.HashPassword(request.NewPassword);

        context.Users.Update(user);
        await context.SaveChangesAsync();
        
        await auditService.LogSecurityActionAsync(request.Email, true);

        return Ok(new ApiResponse<string>(true, "Password altered", "Password reset executed successfully."));
    }
    catch (Exception ex)
    {
        return StatusCode(500, new ApiResponse<string>(false, null, $"Password Reset Error: {ex.Message}"));
    }
}

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value 
                        ?? User.Identity?.Name;

            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(new ApiResponse<string>(false, null, "Unable to extract identity context from session."));
            }
            
            await auditService.LogSecurityActionAsync(email, true, "Manual Session Invalidation");

            return Ok(new ApiResponse<string>(true, "Token dropped", "Logout logged successfully. "));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<string>(false, null, $"Logout Invalidation Error: {ex.Message}"));
        }
    }

}
