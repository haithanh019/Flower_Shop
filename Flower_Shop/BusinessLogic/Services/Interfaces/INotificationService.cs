namespace BusinessLogic.Services.Interfaces
{
    public interface INotificationService
    {
        Task SendMessageAsync(string message);
    }
}
