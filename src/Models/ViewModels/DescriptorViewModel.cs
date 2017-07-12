using Newtonsoft.Json;

namespace Games.Models.ViewModels
{
    public class DescriptorViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string ShortName { get; set; }

        public int UserId { get; set; }
    }
}
