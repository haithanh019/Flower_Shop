namespace BusinessLogic.Services.Interfaces
{
    public interface IEventPublisher
    {
        Task PublishAsync<T>(T eventData);
    }
}
