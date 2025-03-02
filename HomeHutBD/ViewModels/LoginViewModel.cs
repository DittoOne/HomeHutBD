using System.ComponentModel.DataAnnotations;

namespace HomeHutBD.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        // Optionally add RememberMe
        public bool RememberMe { get; set; } = false;
    }
}
