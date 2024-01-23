using System.ComponentModel.DataAnnotations;

namespace IdentityApp_WebApi.DTOs.Account
{
    public class RegisterDto
    {
        [Required]
        [StringLength(15,MinimumLength =3,ErrorMessage ="First name must be at least{3},and maximum {15} characters")]
        public string FirstName { get; set; }
        [Required]
        [StringLength(15, MinimumLength = 3, ErrorMessage = "First name must be at least{3},and maximum {15} characters")]
        public string LastName { get; set; }
        [Required]
        [RegularExpression("^\\w+@[a-zA-Z_]+?\\.[a-zA-Z]{2,3}$",ErrorMessage ="Invalid email adress")]
        public string Email { get; set; }
        [Required]
        [StringLength(15, MinimumLength = 8, ErrorMessage = "First name must be at least{8},and maximum {15} characters")]
        public string  Password { get; set; }
    }
}
