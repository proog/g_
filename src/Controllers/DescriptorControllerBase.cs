using System;
using System.Collections.Generic;
using System.Linq;
using Games.Interfaces;
using Games.Models;
using Games.Models.ViewModels;
using Games.Services;
using Microsoft.AspNetCore.Mvc;

namespace Games.Controllers
{
    public abstract class DescriptorControllerBase<T> : ControllerBase where T : Descriptor, new()
    {
        private readonly GamesContext dbContext;
        private readonly IViewModelFactory vmFactory;

        protected DescriptorControllerBase(GamesContext dbContext, IViewModelFactory vmFactory)
        {
            this.dbContext = dbContext;
            this.vmFactory = vmFactory;
        }

        protected ActionResult<DescriptorViewModel> Add(int userId, DescriptorViewModel vm)
        {
            var user = dbContext.Users.Find(userId);

            var dbSet = dbContext.Set<T>();
            var descriptor = new T
            {
                Name = vm.Name,
                ShortName = vm.ShortName,
                User = user,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            dbSet.Add(descriptor);
            dbContext.SaveChanges();

            var @event = new Event("DescriptorAdded", new { descriptor.Id, descriptor.Name, descriptor.ShortName }, user);
            dbContext.Events.Add(@event);
            dbContext.SaveChanges();

            return vmFactory.MakeDescriptorViewModel(descriptor);
        }

        protected ActionResult<DescriptorViewModel> Update(int userId, int id, DescriptorViewModel vm)
        {
            var user = dbContext.Users.Find(userId);

            var dbSet = dbContext.Set<T>();
            var descriptor = dbSet.FirstOrDefault(x => x.UserId == user.Id && x.Id == id);

            if (descriptor == null)
                return NotFound();

            descriptor.Name = vm.Name;
            descriptor.ShortName = vm.ShortName;
            descriptor.UpdatedAt = DateTime.UtcNow;

            var @event = new Event("DescriptorUpdated", new { descriptor.Id, descriptor.Name, descriptor.ShortName }, user);
            dbContext.Events.Add(@event);
            dbContext.SaveChanges();

            return vmFactory.MakeDescriptorViewModel(descriptor);
        }

        protected ActionResult Delete(int userId, int id)
        {
            var user = dbContext.Users.Find(userId);

            var dbSet = dbContext.Set<T>();
            var descriptor = dbSet.FirstOrDefault(x => x.UserId == user.Id && x.Id == id);

            if (descriptor == null)
                return NotFound();

            dbSet.Remove(descriptor);
            dbContext.Events.Add(new Event("DescriptorDeleted", new { descriptor.Id, descriptor.Name, descriptor.ShortName }, user));
            dbContext.SaveChanges();

            return NoContent();
        }

        protected ActionResult<List<DescriptorViewModel>> All(int userId)
        {
            var user = dbContext.Users.Find(userId);

            if (user == null)
                return NotFound();

            return dbContext.Set<T>()
                .Where(x => x.UserId == userId)
                .Select(vmFactory.MakeDescriptorViewModel)
                .ToList();
        }

        protected ActionResult<DescriptorViewModel> Single(int userId, int id)
        {
            var user = dbContext.Users.Find(userId);

            if (user == null)
                return NotFound();

            var descriptor = dbContext.Set<T>().FirstOrDefault(x => x.UserId == user.Id && x.Id == id);

            if (descriptor == null)
                return NotFound();

            return vmFactory.MakeDescriptorViewModel(descriptor);
        }
    }
}
