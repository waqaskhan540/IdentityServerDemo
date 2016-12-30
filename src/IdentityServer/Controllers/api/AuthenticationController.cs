using AspIdentityClient.Models;
using AspIdentityClient.Models.AccountViewModels;
using AspIdentityClient.Services;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AspIdentityClient.Controllers.api
{
    [Produces("application/json")]
    [Route("api/account")]
    [AllowAnonymous]
    public class AuthenticationController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly ILogger _logger;
        private readonly IIdentityServerInteractionService _interaction;

        public AuthenticationController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            ISmsSender smsSender,
            IIdentityServerInteractionService interaction,
            ILoggerFactory loggerFactory)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _interaction = interaction;
            _logger = loggerFactory.CreateLogger<AuthenticationController>();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("/api/account/Register")]
        // [ValidateAntiForgeryToken]
        public async Task<BaseModel> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation(3, "User created a new account with password.");

                    return new BaseModel
                    {
                        success = true,
                        message = "Registeration successfull"
                    };
                }
                return new BaseModel
                {
                    success = false,
                    error = result.Errors.FirstOrDefault() != null ? result.Errors.FirstOrDefault().Description : "Registeration failed."
                };
            }

            // If we got this far, something failed, redisplay form
            return new BaseModel
            {
                success = false,
                error = "Registeration failed, Invalid credentials"
            };
        }

        [HttpPost]
        [Route("/api/account/GeneratePasswordResetToken")]
        public async Task<BaseModel> GeneratePasswordResetToken(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return new BaseModel
                {
                    success = false,
                    message = "No user exists with the specified email"
                };
            }

            var resetCode = await _userManager.GeneratePasswordResetTokenAsync(user);

            return new BaseModel
            {
                success = true,
                data = resetCode
            };
        }

        [HttpPost]
        [Route("/api/account/ResetPassword")]
        public async Task<BaseModel> ResetPassword(string email, string token, string newPassword)
        {
            var user = _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return new BaseModel
                {
                    success = false,
                    message = "No user exists with the specified email address."
                };
            }

            var result = await _userManager.ResetPasswordAsync(user.Result, token, newPassword);

            if (result.Succeeded)
            {
                return new BaseModel
                {
                    success = true,
                    message = "Password was reset successfully."
                };
            }

            return new BaseModel
            {
                success = true,
                message = result.Errors.FirstOrDefault() != null ? result.Errors.FirstOrDefault().Description : "Password reset failed"
            };
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("/api/account/GetUserId")]
        public async Task<BaseModel> GetUserId(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = await _userManager.FindByNameAsync(email);
                if (user == null)
                {
                    return new BaseModel
                    {
                        success = false,
                        message = "No user exists with the specified username"
                    };
                }
                return new BaseModel
                {
                    success = true,
                    data = user.Id
                };
            }
            return new BaseModel
            {
                success = true,
                data = user.Id
            };
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("/api/account/addusertorole")]
        public async Task<BaseModel> AddUserToRole(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new BaseModel
                {
                    success = false,
                    message = "user not found with specified Id"
                };
            }

            var result = await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, roleName));

            if (result.Succeeded)
            {
                return new BaseModel
                {
                    success = true,
                    message = "Role added"
                };
            }

            return new BaseModel
            {
                success = false,
                message = result.Errors.FirstOrDefault() != null ? result.Errors.FirstOrDefault().Description : "failed to add role"
            };
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("/api/account/userexists")]
        public async Task<BaseModel> UserExists(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return new BaseModel
                {
                    success = true,
                    message = "user exists."
                };
            }

            return new BaseModel
            {
                success = false,
                message = "user doesn't exist."
            };
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("/api/account/GetPasswordFailuresSinceLastSuccess")]
        public async Task<BaseModel> GetPasswordFailuresSinceLastSuccess(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(userName);
                if (user == null)
                {
                    return new BaseModel
                    {
                        success = false,
                        message = "User doesn't exist with the specified username"
                    };
                }
            }

            var failedAttempts = _userManager.GetAccessFailedCountAsync(user).Result;

            return new BaseModel
            {
                success = true,
                message = "Total failed access count:" + failedAttempts.ToString(),
                data = failedAttempts
            };
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("/api/account/ChangePassword")]
        public async Task<BaseModel> ChangePassword(string email, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = await _userManager.FindByNameAsync(email);
                if (user == null)
                {
                    return new BaseModel
                    {
                        success = true,
                        message = "No user exists with the specified email address"
                    };
                }
            }

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

            if (result.Succeeded)
            {
                return new BaseModel
                {
                    success = true,
                    message = "Password changed successfully"
                };
            }

            return new BaseModel
            {
                success = false,
                message = result.Errors.FirstOrDefault() != null ? result.Errors.FirstOrDefault().Description : "failed to change password"
            };
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("/api/account/GenerateEmailVerificationToken")]
        public async Task<BaseModel> GenerateEmailVerificationToken(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return new BaseModel
                {
                    success = false,
                    message = "No user exists with the specified email address"
                };
            }

            var varficationCode = _userManager.GenerateEmailConfirmationTokenAsync(user).Result;

            if (string.IsNullOrWhiteSpace(varficationCode))
            {
                return new BaseModel
                {
                    success = false,
                    message = "Email varification could not be generated."
                };
            }
            var response = await _userManager.ConfirmEmailAsync(user, varficationCode);
            return new BaseModel
            {
                success = true,
                message = "Email verification token generated.",
                data = varficationCode
            };
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("/api/account/removeuserrole")]
        public async Task<BaseModel> RemoveUserFromRole(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return new BaseModel
                {
                    success = false,
                    message = "No user exists with the specified Id"
                };
            }

            var claims = _userManager.GetClaimsAsync(user).Result;

            var roleClaim = claims.Where(x => x.Type == ClaimTypes.Role && x.Value == roleName).FirstOrDefault();

            if (roleClaim == null)
            {
                return new BaseModel
                {
                    success = false,
                    message = "User does not have the specified role assigned."
                };
            }

            var result = _userManager.RemoveClaimAsync(user, new Claim(ClaimTypes.Role, roleName)).Result;

            if (result.Succeeded)
            {
                return new BaseModel
                {
                    success = true,
                    message = "Role successfully removed"
                };
            }

            return new BaseModel
            {
                success = false,
                message = "There was an error removing role from the user,please try again."
            };
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("/api/account/VerifyEmail")]
        public async Task<BaseModel> VerifyEmail(string userId, string verificationCode)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return new BaseModel
                {
                    success = false,
                    message = "No user exists with the specified email address"
                };
            }

            //var decodedtoken = System.Net.WebUtility.UrlDecode(verificationCode);
            var response = await _userManager.ConfirmEmailAsync(user, verificationCode);

            if (response.Succeeded)
            {
                return new BaseModel
                {
                    success = true,
                    message = "Email confirmed."
                };
            }

            return new BaseModel
            {
                success = false,
                message = response.Errors.FirstOrDefault() != null ? response.Errors.FirstOrDefault().Description : "Email confirmation failed."
            };
        }
    }
}