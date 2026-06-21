using Microsoft.AspNetCore.Mvc;
using LoanManagementSystem.Application.DTOs;

namespace LoanManagementSystem.Api.Controllers;

public abstract class BaseController : ControllerBase
{
  
    protected async Task<IActionResult> ExecuteAsync<T>(Func<Task<T>> action)
    {
        try
        {
            var result = await action();
            return Ok(new ApiResponse<T>(true, result, "Success"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse<string>(false, null, ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<string>(false, null, $"Runtime Error: {ex.Message}"));
        }
    }

}