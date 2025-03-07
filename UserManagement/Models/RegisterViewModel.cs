using System.ComponentModel.DataAnnotations;

namespace UserManagement.Models
{
    public class RegisterViewModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6, ErrorMessage = "密碼長度至少 6 個字元")]
        public string Password { get; set; } = string.Empty;

        [StringLength(15)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "User"; // 預設一般使用者
    }
}
