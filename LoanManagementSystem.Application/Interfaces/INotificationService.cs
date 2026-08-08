namespace LoanManagementSystem.Application.Interfaces;

public interface INotificationService
{
    Task BroadcastActivityAsync(object activityData);
    Task BroadcastLoginAsync(object loginData);
}