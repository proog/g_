using System.Linq;
using Games.Interfaces;
using Games.Models;
using Games.Services;
using Microsoft.EntityFrameworkCore;

namespace Games.Repositories
{
    class ConfigRepository : IConfigRepository
    {
        private readonly GamesContext db;

        public ConfigRepository(GamesContext db)
        {
            this.db = db;
        }

        public bool IsConfigured => DefaultConfig != null;

        public Config DefaultConfig => db.Configs.Include(x => x.DefaultUser).SingleOrDefault();

        public void Configure(User defaultUser, string giantBombApiKey)
        {
            var config = DefaultConfig ?? new Config();

            config.DefaultUser = defaultUser;
            config.GiantBombApiKey = giantBombApiKey;

            if (IsConfigured)
                db.Configs.Update(config);
            else
                db.Configs.Add(config);

            db.SaveChanges();
        }
    }
}
