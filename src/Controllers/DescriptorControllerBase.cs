using System;
using System.Collections.Generic;
using System.Linq;
using Games.Infrastructure;
using Games.Interfaces;
using Games.Models;
using Games.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Games.Controllers
{
    public abstract class DescriptorControllerBase : ControllerBase
    {
        private readonly IUserRepository userRepository;
        private readonly IEventRepository eventRepository;
        private readonly IViewModelFactory vmFactory;

        protected DescriptorControllerBase(IUserRepository userRepository, IEventRepository eventRepository, IViewModelFactory vmFactory)
        {
            this.userRepository = userRepository;
            this.eventRepository = eventRepository;
            this.vmFactory = vmFactory;
        }

        protected ActionResult<DescriptorViewModel> Add<T>(int userId, DescriptorViewModel vm, IDescriptorRepository<T> repository) where T : Descriptor, new()
        {
            var user = userRepository.Get(userId);
            var descriptor = new T
            {
                Name = vm.Name,
                ShortName = vm.ShortName,
                User = user,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            repository.Add(descriptor);
            eventRepository.Add(new Event("DescriptorAdded", new { descriptor.Id, descriptor.Name, descriptor.ShortName }, user));

            return vmFactory.MakeDescriptorViewModel(descriptor);
        }

        protected ActionResult<DescriptorViewModel> Update<T>(int userId, int id, DescriptorViewModel vm, IDescriptorRepository<T> repository) where T : Descriptor
        {
            var user = userRepository.Get(userId);
            var descriptor = repository.Get(user, id);

            if (descriptor == null)
                return NotFound();

            descriptor.Name = vm.Name;
            descriptor.ShortName = vm.ShortName;
            descriptor.UpdatedAt = DateTime.UtcNow;

            repository.Update(descriptor);
            eventRepository.Add(new Event("DescriptorUpdated", new { descriptor.Id, descriptor.Name, descriptor.ShortName }, user));

            return vmFactory.MakeDescriptorViewModel(descriptor);
        }

        protected ActionResult Delete<T>(int userId, int id, IDescriptorRepository<T> repository) where T : Descriptor
        {
            var user = userRepository.Get(userId);
            var descriptor = repository.Get(user, id);

            if (descriptor == null)
                return NotFound();

            repository.Delete(descriptor);
            eventRepository.Add(new Event("DescriptorDeleted", new { descriptor.Id, descriptor.Name, descriptor.ShortName }, user));

            return NoContent();
        }

        protected ActionResult<List<DescriptorViewModel>> All<T>(int userId, IDescriptorRepository<T> repository) where T : Descriptor
        {
            var user = userRepository.Get(userId);

            if (user == null)
                return NotFound();

            return repository.All(user)
                .Select(vmFactory.MakeDescriptorViewModel)
                .ToList();
        }

        protected ActionResult<DescriptorViewModel> Single<T>(int userId, int id, IDescriptorRepository<T> repository) where T : Descriptor
        {
            var user = userRepository.Get(userId);

            if (user == null)
                return NotFound();

            var descriptor = repository.Get(user, id);

            if (descriptor == null)
                return NotFound();

            return vmFactory.MakeDescriptorViewModel(descriptor);
        }
    }
}
