using System.ComponentModel.DataAnnotations;

namespace nsia.Models
{
    public class AdminUser
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(100)]
        public string FullName { get; set; } = default!;

        [Required, EmailAddress, MaxLength(150)]
        public string Email { get; set; } = default!;

        [Required]
        public string PasswordHash { get; set; } = default!;

        [MaxLength(30)]
        public string Role { get; set; } = "Admin"; // Admin | SuperAdmin

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
    }
}