using IdentityApp_WebApi.DTOs.Account;
using IdentityApp_WebApi.Models;
using IdentityApp_WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace IdentityApp_WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly JWTService _jwtService;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly EmailService _emailService;
        private readonly IConfiguration _config;

        public AccountController(JWTService jwtService,
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            EmailService emailService,
            IConfiguration config)
       
        {
            _jwtService = jwtService;
            _signInManager = signInManager;
            _userManager = userManager;
            _emailService = emailService;
            _config = config;
        }

        [Authorize]
        [HttpGet("refresh-user-token")]
        public async Task<ActionResult<UserDto>> RefreshUserToken()
        {
            var user = await _userManager.FindByNameAsync(User.FindFirst(ClaimTypes.Email)?.Value);
            return CreateApplicationUserDto(user);
        }
        
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if(user is null)
            {
                
                return Unauthorized("Invalid username or password ");
            }
            if (user.EmailConfirmed == false)
            {
                return Unauthorized("Please confirm your email.");
            }
            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded) { return Unauthorized("Invalid userName or password"); }
            return CreateApplicationUserDto(user);
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterDto model)
        {
            if(await CheckEmailExistsAsync(model.Email))
            {
                return BadRequest($"An existing account is using {model.Email}, email adress .please try with another email adress");
            }
            var userToAdd = new User
            {
                FirstName = model.FirstName.ToLower(),
                LastName = model.LastName.ToLower(),
                UserName = model.Email.ToLower(),
                Email = model.Email.ToLower(),
               
            };
            var result = await _userManager.CreateAsync(userToAdd, model.Password);
            if (result.Succeeded !=true)
            {
                return BadRequest(" your request is not Succeeded "+result.Errors);
            }
            try
            {
                if(await SendConfirmEmailAsync(userToAdd))
                {
                  return Ok(new JsonResult(new { title = "Account Created", message = "your account has been created, Please Confirm your email Adress" }));
                }
                return BadRequest("Faild to send email please contact the admin");
            }
            catch (Exception)
            {
                return BadRequest("Faild to send email please contact the admin");
            }
            
        }

        [HttpPut("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if(user is null)
            {
                return Unauthorized("this email adress has been not registered yet ");
            }

            if (user.EmailConfirmed == true)
            {
                return BadRequest("your email adress was confirmed before ,Please Try Login To your account ");
            }
            try
            {
                var decodedTokenBytes = WebEncoders.Base64UrlDecode(model.Token);
                var decodedToken =Encoding.UTF8.GetString(decodedTokenBytes);
                var resault = await _userManager.ConfirmEmailAsync(user, decodedToken);
                if (resault.Succeeded)
                {
                    return Ok(new JsonResult(new {title="EmailComfirmed",message="Your Email address is Confirmed .You Can Login"}));
                }
                return BadRequest("Invalid token,Please Try Again ");
            }
            catch(Exception){
                return BadRequest("Invalid token,Please Try Again ");
            }

        }

        #region Private Helper Methods
        private UserDto CreateApplicationUserDto(User user)
        {
            return new UserDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                JWT = _jwtService.CreateJWT(user)
            };
        }

        private async Task<bool> CheckEmailExistsAsync(string email)
        {
            return await _userManager.Users.AnyAsync(x => x.Email == email.ToLower());
        }

        private async Task<bool> SendConfirmEmailAsync(User user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var url = $"{_config["JWT:ClientUrl"]}/{_config["Email:ConfirmationEmailPath"]}?token={token}&email={user.Email}";
            var body = $"<p> Hello : {user.FirstName} {user.LastName}</p>"+
                "<p> please Confirem your email by clicking on the following link  </p>"+
                $"<p><a href=\"{url}\"> Click Here </a></p>"+
                "<p>Tank You </p>"+
                $"<br>{_config["Email:ApplicationName"]}";

            var emailSend = new EmailSendDto(user.Email, "Confirm your Email", body);
            return await _emailService.SendEmailAsync(emailSend);

        }
        #endregion
    }

}
