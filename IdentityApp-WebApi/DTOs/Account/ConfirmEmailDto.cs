using System.ComponentModel.DataAnnotations;

namespace IdentityApp_WebApi.DTOs.Account
{
    public class ConfirmEmailDto
    {
        [Required]
        public string Token { get; set; }
        [Required]
        [RegularExpression("^\\w+@[a-zA-Z_]+?\\.[a-zA-Z]{2,3}$", ErrorMessage = "Invalid email adress")]
        public string Email { get; set; }
    }
}
