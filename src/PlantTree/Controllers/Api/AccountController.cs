using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthTokenServer.Common;
using Common.Results;
using Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PlantTree.Data.Entities;
using PlantTree.Infrastructure.Common;
using PlantTree.Models.Api;

namespace PlantTree.Controllers.Api
{
    [Produces("application/json")]
    [Route("api/account")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private ILogger _logger;
        private readonly IEmailSender _emailSender;

        public AccountController(UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager, ILoggerFactory loggerFactory, IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = loggerFactory.CreateLogger<AccountController>();
        }

        // POST: /api/account/register
        /// <summary>
        /// Accepts requests with Content-Type: application/json and body: {"email":"my@my.ru", "password": "mypassword"}
        /// </summary>
        /// <returns></returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var user = new ApplicationUser {UserName = model.Email, Email = model.Email};
            var claim = new IdentityUserClaim<string>()
            {
                ClaimType = ClaimTypes.Role,
                ClaimValue = UserRoles.Local
            };
            user.Claims.Add(claim);
            var result = await _userManager.CreateAsync(user, model.Password);            
            if (result.Succeeded)
            {
                await SendConfirmationEmail(user);
                return new StatusCodeResult(StatusCodes.Status201Created);
            }
            else
                return new CodeWithContentResult(result.Errors, StatusCodes.Status409Conflict);
            //return CreatedAtAction("GetUser", new { id = user.Id }, user);
        }

        /// <summary>
        /// Changes password for local user.
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpPut("password/{current}/{newpass}")]
        public async Task<IActionResult> ChangePassword([FromRoute] string current, [FromRoute] string newpass)
        {
            var applicationUser = await _userManager.GetUserAsync(User);
            if (!await _userManager.HasPasswordAsync(applicationUser))
                return new ApiErrorResult("Can't change password for social signed in user.");

            var result = await _userManager.ChangePasswordAsync(applicationUser, current, newpass);
            if (!result.Succeeded)
            {
                return new ApiErrorResult(string.Join(";",result.Errors.Select(e => e.Description)));
            }

            return Ok();
        }

        [HttpPost("forgot/{email}")]
        public async Task<IActionResult> SendResetMail(string email)
        {
            var user = await _userManager.FindByNameAsync(email);
            if (user == null)// || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                return new ApiErrorResult("No such user or email is not confirmed");
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action("ResetPassword", "Account", new { mail = email, code = code }, protocol: HttpContext.Request.Scheme);
            await _emailSender.SendEmailAsync(email, "Reset Password",
               $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");
            return Ok(new ApiSuccess("Forgot mail was sent."));
        }

        [Authorize]
        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmationEmail()
        {
            var applicationUser = await _userManager.GetUserAsync(User);
            if (!await _userManager.HasPasswordAsync(applicationUser))
                return new ApiErrorResult("No need to confirm email for social signed in user.");
            await SendConfirmationEmail(applicationUser);
            return Ok(new ApiSuccess("Confirmation email was sent"));
        }

        [NonAction]
        public async Task SendConfirmationEmail(ApplicationUser user)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
            await _emailSender.SendEmailAsync(user.Email, "Confirm your account on Tooba",
                $"Please confirm your Tooba account by clicking this link: <a href='{callbackUrl}'>link</a>");
        }

        [HttpGet("info")]
        [Authorize]
        public async Task<IActionResult> GetUserInfo()
        {
            var user = await _userManager.GetUserAsync(User);
            return Ok(new
            {
                name = user.Name,
                email = user.Email,
                isEmailConfirmed = user.EmailConfirmed,
                gender = user.Gender?.ToString(),
                birthday = user.Birthday?.ToString("dd.MM.yyyy"),
                //likes = user.ProjectUsers.Count
            });
        }

        [HttpPut("info")]
        [Authorize]
        public async Task<IActionResult> SetUserInfo([FromBody] UserInfoModel info)
        {
            await Misc.SetUserInfo(HttpContext, info);
            return NoContent();
        }

        [HttpPost("photo")]
        //[Authorize]
        public async Task<IActionResult> SetUserPhoto(IFormFile photo, [FromServices] ImageFactory imageFactory)
        {
            _logger.LogInformation(Request.Form.Keys.Count.ToString());
            _logger.LogInformation(String.Concat(Request.Form.Keys));
            _logger.LogInformation($"Photo action: name: {photo?.Name}, length: {photo?.Length}, photo is null: {photo == null}");
            // TODO: test from mobile devices
            if (photo != null)
            {
                var image = await imageFactory.CreateUserImage(photo);
                return Content($"Image <a href='{image.Url}'>{photo.FileName}</a> was uploaded: <img src='{image.Url}' />", "text/html");
            }

            return BadRequest();
        }
    }
}