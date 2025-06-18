
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Interfracture.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string? FcmToken { get; set; }
        public DateTime DateOfBirth { get; set; }
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
