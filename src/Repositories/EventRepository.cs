using System;
using Games.Interfaces;
using Games.Models;
using Games.Services;

namespace Games.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly GamesContext db;

        public EventRepository(GamesContext db)
        {
            this.db = db;
        }

        public void Add(Event @event)
        {
            @event.CreatedAt = DateTime.UtcNow;

            db.Events.Add(@event);
            db.SaveChanges();
        }
    }
}