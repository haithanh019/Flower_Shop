using BusinessLogic.Services.Interfaces;

namespace BusinessLogic.Services
{
    public class InMemoryEventBus : IEventPublisher
    {
        public static event Func<object, Task>? OnPublish;

        public async Task PublishAsync<T>(T eventData)
        {
            if (OnPublish != null && eventData != null)
            {
                await OnPublish.Invoke(eventData);
            }
        }
    }
}
