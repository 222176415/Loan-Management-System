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
            
            var user = await context.Users
                .IgnoreQueryFilters() 
                .FirstOrDefaultAsync(u => u.Email == request.Email);

         
            if (user == null || !PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                await auditService.LogLoginAsync(request.Email, false, "Invalid credentials");
                return Unauthorized(new ApiResponse<string>(false, null, "Invalid email or password."));
            }

            if (!user.IsActive)
            {
                await auditService.LogLoginAsync(request.Email, false, "Account deactivated");
                return BadRequest(new ApiResponse<string>(false, null, "Your account is deactivated."));
            }

          
            var token = GenerateJwtToken(user);

        
            await auditService.LogLoginAsync(request.Email, true);

            var response = new AuthResponse(token, user.Email, user.OrganizationId);
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
}
