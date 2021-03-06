using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthTokenServer.Common;
using Common.Errors;
using Common.Results;
using Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PlantTree.Data;
using PlantTree.Data.Entities;
using PlantTree.Infrastructure.Common;
using PlantTree.Models.Api;

namespace PlantTree.Controllers.Api
{
    [Area("api")]
    [Produces("application/json")]
    [Route("api/account")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger _logger;
        private readonly IEmailSender _emailSender;

        public AccountController(UserManager<ApplicationUser> userManager, AppDbContext context,
            SignInManager<ApplicationUser> signInManager, ILoggerFactory loggerFactory, IEmailSender emailSender)
        {
            _userManager = userManager;
            _context = context;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = loggerFactory.CreateLogger<AccountController>();
        }

        // POST: /api/account/register
        /// <summary>
        /// Accepts requests with Content-Type: application/json and body: {"email":"my@my.ru", "password": "mypassword"}
        /// </summary>
        /// <response code="201">User created</response>
        /// <response code="409">User created</response>
        /// <returns></returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiError[]), 409)]
        [Consumes("application/json")]
        public async Task<IActionResult> Register([FromBody] RegisterModel registerInfo)
        {
            // TODO: add validation for registerInfo
            var user = new ApplicationUser { UserName = registerInfo.Email, Email = registerInfo.Email };
            var claim = new IdentityUserClaim<string>()
            {
                ClaimType = ClaimTypes.Role,
                ClaimValue = UserRoles.Local
            };
            user.Claims.Add(claim);
            var result = await _userManager.CreateAsync(user, registerInfo.Password);
            if (result.Succeeded)
            {
                try
                {
                    await SendConfirmationEmail(user);
                }
                catch
                {
                    // Suppress error
                }
                return new StatusCodeResult(StatusCodes.Status201Created);
            }
            else
            {
                var errors = new List<ApiError>();
                foreach (var identityError in result.Errors)
                {
                    var errorType = ApiErrorTypes.System;
                    if (identityError.Code == "DuplicateUserName" || identityError.Code == "PasswordTooShort")
                        errorType = ApiErrorTypes.User;
                    var apiError = new ApiError(identityError.Description, identityError.Code, errorType);
                    errors.Add(apiError);
                }
                return new ApiErrorResult(errors.ToArray()) { StatusCode = StatusCodes.Status409Conflict };
            }
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
                return new ApiErrorResult(result.Errors.Select(e =>
                    new ApiError(e.Description, e.Code, 
                                 e.Code == "PasswordMismatch" ? ApiErrorTypes.User : ApiErrorTypes.System)).ToArray());
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
            try
            {
                await _emailSender.SendEmailAsync(email, "Reset Password",
                $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");
            }
            catch (ApiException e)
            {
                return new ApiErrorResult(e.Errors.ToArray());
            }

            return Ok(new ApiSuccess("Forgot mail was sent."));
        }

        [Authorize]
        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmationEmail()
        {
            var applicationUser = await _userManager.GetUserAsync(User);
            if (!await _userManager.HasPasswordAsync(applicationUser))
                return new ApiErrorResult("No need to confirm email for social signed in user.");
            try
            {
                await SendConfirmationEmail(applicationUser);
            }
            catch (ApiException e)
            {
                return new ApiErrorResult(e.Errors.ToArray());
            }
            catch (Exception e)
            {
                return new ApiErrorResult(e.ToString());
            }
            return Ok(new ApiSuccess("Confirmation email was sent"));
        }

        [NonAction]
        public async Task SendConfirmationEmail(ApplicationUser user)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
            await _emailSender.SendEmailAsync(user.Email, "Confirm your account on PlantTree",
                $"Please confirm your PlantTree account by clicking this link: <a href='{callbackUrl}'>link</a>");
        }

        [HttpGet("info")]
        [Authorize]
        [ProducesResponseType(typeof(DetailedUserInfo), 200)]
        public async Task<IActionResult> GetUserInfo()
        {
            //await _userManager.GetUserAsync(User);
            var user = await _context.ApplicationUser
                .Include(u => u.Photo)
                .SingleOrDefaultAsync(u => u.Id == _userManager.GetUserId(User));
            return Ok(new DetailedUserInfo()
            {
                Name = user.Name,
                LastName = user.LastName,
                Email = user.Email,
                IsEmailConfirmed = user.EmailConfirmed,
                Gender = user.Gender?.ToString(),
                Birthday = user.Birthday?.ToString("dd.MM.yyyy"),
                Donated = user.Donated,
                DonatedProjectsCount = user.DonatedProjectsCount,
                PhotoUrl = user.PhotoUrl,
                PhotoUrlSmall = user.PhotoUrlSmall
            });
        }

        /// <summary>
        /// DateTime format - "dd.MM.yyyy"
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        [HttpPut("info")]
        [Authorize]
        public async Task<IActionResult> SetUserInfo([FromBody] UserInfo info)
        {
            await Misc.SetUserInfo(HttpContext, info);
            return NoContent();
        }

        [HttpPost("photo")]
        [Authorize]
        //[ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> SetUserPhoto(IFormFile photo, [FromServices] ImageFactory imageFactory)
        {
            //_logger.LogInformation(Request.Form.Keys.Count.ToString());
            //_logger.LogInformation(String.Concat(Request.Form.Keys));
            //_logger.LogInformation($"Photo action: name: {photo?.Name}, length: {photo?.Length}, photo is null: {photo == null}");
            if (photo == null) return BadRequest();

            var createImageTask = imageFactory.CreateUserImage(photo);
            var getUserTask = _userManager.GetUserAsync(User);

            var image = await createImageTask;
            var user = await getUserTask;
            user.Photo = image;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded) return Ok();
            return new ApiErrorResult(result.Errors.Select(e => new ApiError(e.Description, e.Code)).ToArray());
        }

        /// <summary>
        /// Deletes current user photo
        /// </summary>
        /// <param name="imageFactory"></param>
        /// <response code="204">Success</response>
        /// <returns></returns>
        [HttpDelete("photo")]
        [Authorize]
        public async Task<IActionResult> DeleteUserPhoto([FromServices] ImageFactory imageFactory)
        {
            //_logger.LogInformation(Request.Form.Keys.Count.ToString());
            //_logger.LogInformation(String.Concat(Request.Form.Keys));
            //_logger.LogInformation($"Photo action: name: {photo?.Name}, length: {photo?.Length}, photo is null: {photo == null}");
            var user = await _context.Users.Include(u => u.Photo).SingleAsync(u => u.Id == _userManager.GetUserId(User));
            if (user.PhotoId.HasValue)
            {
                imageFactory.RemoveImageFiles(user.Photo);
                _context.Remove(user.Photo);
                user.Photo = null;
                await _context.SaveChangesAsync();
            }
            return NoContent();
        }


        [HttpPost("phototest")]
        //[Authorize]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> SetUserPhotoTest(IFormFile photo, [FromServices] ImageFactory imageFactory)
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