using System.Collections.Generic;
using Games.Models;

namespace Games.Interfaces
{
    public interface IDescriptorRepository<T> where T : Descriptor
    {
        IEnumerable<T> All(User user);

        T Get(User user, int id);

        void Add(T descriptor);

        void Update(T descriptor);

        void Delete(T descriptor);
    }

    public interface IGenreRepository : IDescriptorRepository<Genre> { }

    public interface IPlatformRepository : IDescriptorRepository<Platform> { }

    public interface ITagRepository : IDescriptorRepository<Tag> { }
}
