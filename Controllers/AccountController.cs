using Gest_Immo_API.DTOs.Account;
using Gest_Immo_API.Models;
using Gest_Immo_API.Services;
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

namespace Gest_Immo_API.Controllers
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
            if (user == null) return Unauthorized("Nom d'utilisateur ou mot de passe incorrect");

            if (!user.EmailConfirmed) return Unauthorized("Veuillez confirmer votre email.");

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded) return Unauthorized("Nom d'utilisateur ou mot de passe incorrect");

            return CreateApplicationUserDto(user);

        }
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            if (await CheckEmailExistsAync(model.Email)) return BadRequest($"Un compte est déjà enregistré avec l'adresse email {model.Email}.");

            var userToAdd = new User { FirstName = model.FirstName.ToLower(), LastName = model.LastName.ToLower(), UserName = model.Email.ToLower(), Email = model.Email.ToLower(), };

            var result = await _userManager.CreateAsync(userToAdd, model.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            try
            {
                if (await SendConfirmEmailAsync(userToAdd))
                {
                    return Ok(new JsonResult(new
                    {
                        title = "Compte enregistré",
                        message = "Un email de confirmation vous a été adressé à l'adresse email indiquée. Veuillez suivre les instructions pour compléter la création de votre compte."
                    }));
                }
                return BadRequest("Echec d'envoi de l'email de confirmation. Veuillez contacter un administrateur");
            }
            catch (Exception)
            {
                return BadRequest("Echec d'envoi de l'email de confirmation. Veuillez contacter un administrateur");
            }

            //return Ok(new JsonResult(new {title="Compte enregistré", 
            //    message= "Votre compte a été correctement créé, vous pouvez vous connecter" }));
            //return Ok("Le compte a été correctement créé, vous pouvez vous connKecter");
        }

        [HttpPut("confirm-email")]
        public async Task<IActionResult> confirmEmail(ConfirmEmailDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return Unauthorized("Cette adresse email n'a pas encore été enregistrée");

            if (user.EmailConfirmed) return BadRequest("Cette adresse email a déjà été confirmée. Vous pouvez vous connecter à votre compte");

            try
            {
                var decodedTokenBytes = WebEncoders.Base64UrlDecode(model.Token);
                var decodeToken = Encoding.UTF8.GetString(decodedTokenBytes);

                var result = await _userManager.ConfirmEmailAsync(user, decodeToken);
                if (result.Succeeded)
                {
                    return Ok(new JsonResult(new { title = "Adresse email confirmée", message = "Votre adresse email est confirmée. Vous pouvez vous connecter à votre compte" }));
                }
                return BadRequest("Token invalide. Veuillez essayer à nouveau");
            }
            catch (Exception)
            {
                return BadRequest("Token invalide. Veuillez essayer à nouveau");
            }

        }

        [HttpPost("resend-email-confirmation-link/{email}")]
        public async Task<IActionResult> ResendEmailConfirmationLink(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Adresse email invalide");
            }
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null) return Unauthorized("Cette adresse email n'a pas encore été enregistrée");
            if (user.EmailConfirmed) return BadRequest("Votre adresse email a déjà été confirmée. Vous pouvez vous connecter à votre compte");

            try
            {
                if (await SendConfirmEmailAsync(user))
                {
                    return Ok(new JsonResult(new
                    {
                        title = "Lien de confirmation envoyé",
                        message = "Un email de confirmation vous a été adressé à l'adresse email indiquée. Veuillez suivre les instructions pour compléter la création de votre compte."
                    }));
                }
                return BadRequest("Echec d'envoi de l'email de confirmation. Veuillez contacter un administrateur");
            }
            catch (Exception)
            {
                return BadRequest("Echec d'envoi de l'email de confirmation. Veuillez contacter un administrateur");
            }

        }

        [HttpPost("forgot-username-or-password/{email}")]
        public async Task<IActionResult> ForgotUsernameOrPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Adresse email invalide");
            }
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return Unauthorized("Cette adresse email n'est pas enregistrée");
            if (!user.EmailConfirmed) return BadRequest("Votre adresse email n'a pas été confirmée. Veuillez confirmer votre adresse email");

            try
            {
                if (await SendForgotUsernameOrPasswordEmail(user))
                {
                    return Ok(new JsonResult(new
                    {
                        title = "Email envoyé",
                        message = "Un email pour mot de passe perdu vous a été adressé à l'adresse email indiquée. Veuillez consulter votre boîte email."
                    }));
                }
                return BadRequest("Echec d'envoi de l'email pour mot de passe perdu. Veuillez contacter un administrateur");
            }
            catch (Exception)
            {
                return BadRequest("Echec d'envoi de l'email pour mot de passe perdu. Veuillez contacter un administrateur");
            }

        }

        [HttpPut("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto model, string email)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return Unauthorized("Cette adresse email n'est pas enregistrée");
            if (!user.EmailConfirmed) return BadRequest("Votre adresse email n'est pas confirmée. Veuillez confirmer votre adresse email");

            try
            {
                var decodedTokenBytes = WebEncoders.Base64UrlDecode(model.Token);
                var decodeToken = Encoding.UTF8.GetString(decodedTokenBytes);

                var result = await _userManager.ResetPasswordAsync(user, decodeToken, model.NewPassword);
                if (result.Succeeded)
                {
                    return Ok(new JsonResult(new { title = "Réinitialisation du mot de passe réussie", message = "Votre mot de passe a été réinitialisé. Vous pouvez vous connecter à votre compte" }));
                }
                return BadRequest("Token invalide. Veuillez essayer à nouveau");
            }
            catch (Exception)
            {
                return BadRequest("Token invalide. Veuillez essayer à nouveau");
            }

        }

        #region private helper methods
        private UserDto CreateApplicationUserDto(User user)
        {
            return new UserDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                JWT = _jwtService.CreateJWT(user),
            };
        }

        private async Task<bool> CheckEmailExistsAync(string email)
        {
            return await _userManager.Users.AnyAsync(x => x.Email == email.ToLower());
        }

        private async Task<bool> SendConfirmEmailAsync(User user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var url = $"{_config["JWT:ClientUrl"]}/{_config["Email:ConfirmEmailPath"]}?token={token}&email={user.Email}";

            var body = $"<p>Bonjour: {user.FirstName} {user.LastName} </p>" +
                "<p>Veuillez confirmer votre adresse email en cliquant sur le lien suivant</p>" +
                $"<p><a href=\"{url}\">Cliquez ici</a></p>" +
                "<p>Merci</p>" +
                $"<br>{_config["Email:ApplicationName"]}";

            var emailSend = new EmailSendDto(user.Email, "Validez votre adresse email", body);

            return await _emailService.SendEmailAsync(emailSend);
        }

        private async Task<bool> SendForgotUsernameOrPasswordEmail(User user)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var url = $"{_config["JWT:ClientUrl"]}/{_config["Email:ResetPasswordPath"]}?token={token}&email={user.Email}";

            var body = $"<p>Bonjour: {user.FirstName} {user.LastName} </p>" +
                $"<p>Nom d'utilisateur: {user.UserName}</p>" +
                "<p>Pour mettre à jour votre mot de passe, cliquez sur le lien suivant:</p>" +
                $"<p><a href=\"{url}\">Cliquez ici</a></p>" +
                "<p>Merci</p>" +
                $"<br>{_config["Email:ApplicationName"]}";

            var emailSend = new EmailSendDto(user.Email, "Nom d'utilisateur ou mot de passe perdu", body);

            return await _emailService.SendEmailAsync(emailSend);
        }
        #endregion
    }
}
