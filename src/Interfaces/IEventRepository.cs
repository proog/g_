using Games.Models;

namespace Games.Interfaces
{
    public interface IEventRepository
    {
        void Add(Event @event);
    }
}