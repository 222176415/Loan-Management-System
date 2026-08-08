using LoanManagementSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoanManagementSystem.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AuditController(
    IAuditService auditService, 
    IExcelService excelService) : BaseController
{
    [HttpGet("export-activities")]
    public async Task<IActionResult> ExportActivities()
    {
        // 1. Fetch the descriptive logs
        var logs = await auditService.GetActivityLogsAsync();

        // 2. Map to a flat structure for Excel
        var exportData = logs.Select(x => new {
            Date = x.Timestamp.ToString("yyyy-MM-dd HH:mm"),
            User = x.UserId,
            Action = x.Action,
            Entity = x.EntityName,
            EntityID = x.EntityId,
            Changes = $"From: {x.OldValue} | To: {x.NewValue}"
        });

        var file = excelService.ExportToExcel(exportData, "Activity Audit Trail");
        return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Activity_Audit_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    [HttpGet("export-logins")]
    public async Task<IActionResult> ExportLogins()
    {
        var logs = await auditService.GetLoginLogsAsync();

        var exportData = logs.Select(x => new {
            Time = x.Timestamp.ToString("yyyy-MM-dd HH:mm"),
            Email = x.UserEmail,
            Status = x.IsSuccess ? "Success" : "Failed",
            IP_Address = x.IpAddress,
            Failure_Reason = x.FailureReason ?? "N/A",
            Device = x.UserAgent
        });

        var file = excelService.ExportToExcel(exportData, "Login Audit Trail");
        return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Login_Audit_{DateTime.Now:yyyyMMdd}.xlsx");
    }
}
