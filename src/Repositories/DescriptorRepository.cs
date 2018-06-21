using System;
using System.Collections.Generic;
using System.Linq;
using Games.Interfaces;
using Games.Models;
using Games.Services;
using Microsoft.EntityFrameworkCore;

namespace Games.Repositories
{
    public abstract class DescriptorRepository<T> : IDescriptorRepository<T> where T : Descriptor
    {
        private readonly GamesContext db;
        private readonly DbSet<T> dbSet;

        protected DescriptorRepository(GamesContext db, DbSet<T> dbSet)
        {
            this.db = db;
            this.dbSet = dbSet;
        }

        public IEnumerable<T> All(User user)
        {
            return dbSet.Where(x => x.UserId == user.Id).ToList();
        }

        public T Get(User user, int id)
        {
            return dbSet.SingleOrDefault(x => x.UserId == user.Id && x.Id == id);
        }

        public void Add(T descriptor)
        {
            dbSet.Add(descriptor);
            db.SaveChanges();
        }

        public void Update(T descriptor)
        {
            descriptor.UpdatedAt = DateTime.UtcNow;
            dbSet.Update(descriptor);
            db.SaveChanges();
        }

        public void Delete(T descriptor)
        {
            dbSet.Remove(descriptor);
            db.SaveChanges();
        }
    }

    class GenreRepository : DescriptorRepository<Genre>, IGenreRepository
    {
        public GenreRepository(GamesContext db) : base(db, db.Genres) { }
    }

    class PlatformRepository : DescriptorRepository<Platform>, IPlatformRepository
    {
        public PlatformRepository(GamesContext db) : base(db, db.Platforms) { }
    }

    class TagRepository : DescriptorRepository<Tag>, ITagRepository
    {
        public TagRepository(GamesContext db) : base(db, db.Tags) { }
    }
}
