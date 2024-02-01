using System.ComponentModel.DataAnnotations;

namespace IdentityApp_WebApi.DTOs.Account
{
    public class LoginDto
    {
        [Required(ErrorMessage ="Username is required")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Username is required")] 
        public string Password { get; set; }
    }
}
