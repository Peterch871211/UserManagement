using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace UserManagement.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        private string _passwordHash = string.Empty;

        [Required]
        [StringLength(255)]
        public string Password
        {
            get => _passwordHash;
            set
            {
                // **避免重複加密**
                if (!string.IsNullOrEmpty(value) && !value.StartsWith("AQAAAAIAAYag"))
                {
                    var hasher = new PasswordHasher<User>();
                    _passwordHash = hasher.HashPassword(this, value);
                }
                else
                {
                    _passwordHash = value; // **如果已經是哈希值，就直接存入**
                }
            }
        }

        [StringLength(15)]
        public string Phone { get; set; } = string.Empty;

        [StringLength(100)]
        public string Role { get; set; } = string.Empty;
    }
}
