using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Laboremus_AuthorizationService.Core.Exceptions;
using Laboremus_AuthorizationService.Core.Extensions;
using Laboremus_AuthorizationService.DTOs;
using Laboremus_AuthorizationService.Models;
using Laboremus_AuthorizationService.Models.AccountViewModels;
using Laboremus_AuthorizationService.Services.EmailSender;
using Laboremus_AuthorizationService.Services.ExportService;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace Laboremus_AuthorizationService.Controllers
{
    /// <inheritdoc />
    /// <summary>
    /// This controller handles everything to do with users
    /// </summary>
    [Route("api/users")]
    public class UsersController : BaseController
    {
        private readonly ILogger<UsersController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IExportService _exportService;
        private readonly IEmailSender _emailSender;

        /// <summary>
        /// </summary>
        /// <param name="userManager"></param>
        /// <param name="logger"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="exportService"></param>
        /// <param name="emailSender"></param>
        public UsersController(
            UserManager<ApplicationUser> userManager, 
            ILogger<UsersController> logger,
            IActionContextAccessor actionContextAccessor,
            IExportService exportService,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _logger = logger;
            _actionContextAccessor = actionContextAccessor;
            _exportService = exportService;
            _emailSender = emailSender;
        }

        /// <summary>
        ///     Handle user registration
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Produces(typeof(UserDetailsViewModel))]
        public async Task<UserDetailsViewModel> Create([FromBody] NewUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                _logger.LogInformation("check if the username is already taken");
                var user = await _userManager.FindByNameAsync(model.PreferredUsername);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = model.PreferredUsername,
                        Email = model.Email,
                        PhoneNumber = model.Telephone,
                    };

                    _logger.LogInformation("create a new user");
                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation($"add the user to the roles {string.Join(",", model.Roles)}");
                        result = await _userManager.AddToRolesAsync(user, model.Roles);

                        if (!result.Succeeded)
                        {
                            _logger.LogError($"adding user to roles failed - {result.Errors.Select(it => it.Description).ToJson()}");
                        }

                        _logger.LogInformation("add the full name to the claims");
                        model.Claims.Add("name", model.Fullname);

                        var claims = model.Claims.Select(s => new Claim(s.Key, s.Value));

                        _logger.LogInformation("add user claims");
                        result = await _userManager.AddClaimsAsync(user, claims);

                        if (!result.Succeeded)
                        {
                            _logger.LogError($"adding user claims failed - {result.Errors.Select(it => it.Description).ToJson()}");
                        }
                    }
                    else
                    {
                        var errors = result.Errors.Select(it => it.Description).ToList();
                        _logger.LogError($"adding user claims failed - {errors.ToJson()}");

                        throw new InvalidModelException(errors.First(), errors);
                    }
                }

                return new UserDetailsViewModel
                {
                    Id = user.Id,
                    Fullname = model.Fullname,
                    PreferredUsername = user.UserName,
                    Email = user.Email,
                    Telephone = user.PhoneNumber,
                    Roles = model.Roles,
                    Claims = model.Claims,
                    LockedOut = user.LockoutEnabled
                };
            }

            var validationErrors = new List<string>();
            foreach (var modelState in ModelState.Values)
            {
                foreach (var error in modelState.Errors)
                {
                    validationErrors.Add(error.ErrorMessage);
                }
            }

            throw new InvalidModelException("Invalid data model", validationErrors);
        }


        /// <summary>
        ///     Send user reset password link
        /// </summary>
        /// <param email="email"></param>
        /// <returns></returns>
        [HttpPost("passwordreset")]
        public async Task<IActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordViewModel model)
        {
            // get user by id
            var user = await _userManager.FindByNameAsync(model.Email);
            if (user == null) return NotFound(model.Email);

            // For more information on how to enable account confirmation and password reset please 
            // visit https://go.microsoft.com/fwlink/?LinkID=532713
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Action(
                    nameof(ResetPassword),
                    "Account",
                    values: new { controller = "Account", code },
                    protocol: Request.Scheme);

            await _emailSender.SendResetPasswordEmailAsync(model.Email, callbackUrl);

            return Ok();
        }

        public IActionResult ResetPassword(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                Response.StatusCode = 404;
                return View("NotFound");
            }

            var model = new ResetPasswordViewModel { Code = code };
            return View(model);
        }

        /// <summary>
        ///     Get details of a user by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Produces(typeof(UserDetailsViewModel))]
        public async Task<IActionResult> Read([FromRoute] string id)
        {
            var details = new UserDetailsViewModel();

            // get the user by ID
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id.Equals(id));
            if (user == null) return NotFound();

            details.Id = user.Id;
            details.Email = user.Email;
            details.PreferredUsername = user.UserName;
            details.Telephone = user.PhoneNumber;
            details.LockedOut = user.LockoutEnabled;

            details.Roles = await _userManager.GetRolesAsync(user);

            // get the user claims
            var claims = await _userManager.GetClaimsAsync(user);
            if (claims?.Count > 0)
            {
                foreach (var claim in claims)
                {
                    if (!details.Claims.ContainsKey(claim.Type))
                    {
                        details.Claims.Add(claim.Type, claim.Value);
                    }
                }
            }

            return Ok(details);
        }

        /// <summary>
        ///     Check if user already exists based on email
        /// </summary>
        /// <param email="email"></param>
        /// <returns></returns>
        [HttpPost("check")]
        public async Task<IActionResult> CheckUserExists([FromBody] UserEmailViewModel model)
        {
            if (model.Email == null) return Ok();

            // get user by email
            var user = await _userManager.FindByNameAsync(model.Email);
            if (user == null) return Ok();

            return Ok(model.Email);
        }

        /// <summary>
        ///     Searches for a user by Id, Email, Name and Username.
        /// </summary>
        /// <param name="request"></param>
        /// <remarks>If search criterion is not specified, it will return the first 50 users</remarks>
        /// <returns></returns>
        [HttpGet]
        [Produces(typeof(ICollection<UserDetailsViewModel>))]
        public async Task<IActionResult> Search(SearchRequest request)
        {
            var query = _userManager.Users;

            var usersList = new List<UserDetailsViewModel>();

            #region Filter users

            if (request.Name != null)
            {
                if (request.Id != null)
                {
                    query = query.Where(q => q.Id == request.Id);
                }

                if (request.Username != null)
                {
                    query = query.Where(q => q.UserName.ToLower().Contains(request.Username.ToLower()));
                }

                if (request.Email != null)
                {
                    query = query.Where(q => q.Email.ToLower().Contains(request.Email.ToLower()));
                }

                var allUsers = await query.ToListAsync();

                foreach (var user in allUsers)
                {
                    var userDetail = new UserDetailsViewModel
                    {
                        Id = user.Id,
                        Email = user.Email,
                        Telephone = user.PhoneNumber,
                        PreferredUsername = user.UserName,
                        Roles = await _userManager.GetRolesAsync(user),
                        LockedOut = user.LockoutEnabled
                    };

                    var claims = await _userManager.GetClaimsAsync(user);

                    if (claims?.Count > 0)
                    {
                        foreach (var claim in claims)
                        {
                            if (!userDetail.Claims.ContainsKey(claim.Type))
                            {
                                userDetail.Claims.Add(claim.Type, claim.Value);
                            }
                        }
                    }

                    usersList.Add(userDetail);
                }

                var skipValue = (request.Page - 1) * request.ItemsPerPage;

                var foundUsers = usersList.Where(u => u.Claims.Any(c => c.Key == "name" && c.Value.ToLower()
                    .Contains(request.Name.ToLower())))
                    .Skip(skipValue)
                    .Take(request.ItemsPerPage)
                    .ToList();

                if (foundUsers.Count == 0) return NotFound();
                
                return Ok(foundUsers);

            }
            else if (request.Id != null)
                query = query.Where(q => q.Id == request.Id);
            else if (request.Username != null)
                query = query.Where(q => q.UserName.ToLower().Contains(request.Username.ToLower()));
            else if (request.Email != null)
                query = query.Where(q => q.Email.ToLower().Contains(request.Email.ToLower()));

            #endregion

            // default Page = 2
            var skip = (request.Page - 1) * request.ItemsPerPage;

            var users = await query.Skip(skip).Take(request.ItemsPerPage).ToListAsync();

            if (users.Count == 0) return NotFound();

            foreach (var user in users)
            {
                var userDetail = new UserDetailsViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    Telephone = user.PhoneNumber,
                    PreferredUsername = user.UserName,
                    Roles = await _userManager.GetRolesAsync(user),
                    LockedOut = user.LockoutEnabled
                };

                var claims = await _userManager.GetClaimsAsync(user);

                if (claims?.Count > 0)
                {
                    foreach (var claim in claims)
                    {
                        if (!userDetail.Claims.ContainsKey(claim.Type))
                        {
                            userDetail.Claims.Add(claim.Type, claim.Value);
                            if (userDetail.Claims.TryGetValue("name", out string nameValue))
                            {
                                userDetail.Fullname = nameValue;
                            }
                        }
                    }
                }

                usersList.Add(userDetail);
            }

            return Ok(usersList);
        }

        /// <summary>
        ///     Update user details
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Produces(typeof(UserDetailsViewModel))]
        public async Task<IActionResult> Update([FromBody] UpdateUserViewModel model)
        {
            _logger.LogInformation($"get the user by User ID: {model.Id}");
            var user = await _userManager.Users.FirstOrDefaultAsync(q => q.Id == model.Id);
            if (user == null)
            {
                _logger.LogInformation("the user was not found");
                return NotFound();
            }

            #region Set new values
            user.UserName = model.PreferredUsername;
            user.Email = model.Email;
            user.PhoneNumber = model.Telephone;
            #endregion

            _logger.LogInformation("update the user details");
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                await UpdateUserClaimsAsync(model, user);
                await UpdateUserRolesAsync(model, user);
            }
            else
            {
                _logger.LogError($"an error occured while trying to update the user {user.Id}: {user.UserName}");
                throw new Exception(result.Errors.Select(s => s.Description).ToJson());
            }

            return Ok();
        }

        /// <summary>
        ///     Activates or deactivates a user
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> ChangeStatus([FromRoute] string id, [FromBody] UserLockOutViewModel model)
        {
            // get user by id
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.LockoutEnabled = model.lockedOut.Value;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded) return Ok();
            return BadRequest(result.Errors);
        }

        /// <summary>
        /// Delete a user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {
            _logger.LogInformation("start deleting a user...");
            _logger.LogInformation($"finding the user to delete by ID: {id}");
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                _logger.LogInformation("the user was not found");
                return NotFound(id);
            }

            _logger.LogInformation("get all roles for the user");
            var roles = await _userManager.GetRolesAsync(user);

            _logger.LogInformation("remove user from all roles");
            var result = await _userManager.RemoveFromRolesAsync(user, roles);

            if (result.Succeeded)
            {
                _logger.LogInformation("get all the user claims");
                var claims = await _userManager.GetClaimsAsync(user);

                _logger.LogInformation("remove all claims from the user");
                result = await _userManager.RemoveClaimsAsync(user, claims);

                if (result.Succeeded)
                {
                    _logger.LogInformation("delete the user");
                    await _userManager.DeleteAsync(user);

                    _logger.LogInformation($"the user {id} has been deleted.");
                    return Ok(id);
                }
            }

            _logger.LogError($"deleting the user failed.");
            throw new Exception("deleting the user failed");
        }

        /// <summary>
        /// Initiate an export of users
        /// </summary>
        /// <remarks>
        /// Accepts request for export verification requests. Request is queued and processed as soon as possible.
        /// </remarks>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost("export")]
        public async Task<IActionResult> CreateExport([FromBody] UserExportRequest request)
        {
            var response = await _exportService.ExportAsync(request);
            return AcceptedAtAction(nameof(GetExportStatus),
                value: PrepareStatusResponse(response, _actionContextAccessor.ActionContext),
                routeValues: new { id = response.Id });
        }

        /// <summary>
        /// Get export status
        /// </summary>
        /// <remarks>Endpoint for getting export status for a specific export request by unique id.</remarks>
        /// <param name="id"></param>
        /// <example>8754b7cb-d0fc-4499-8a1a-ebfb721cf0fc</example>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [ProducesResponseType((int)HttpStatusCode.Accepted, Type = typeof(ExportStatusResponse))]
        [HttpGet("export/{id:Guid}/status", Name = nameof(GetExportStatus))]
        public async Task<IActionResult> GetExportStatus([FromRoute] Guid id)
        {
            var result = await _exportService.CheckRequestStatusAsync(id);

            return Ok(PrepareStatusResponse(result, _actionContextAccessor.ActionContext));
        }

        /// <summary>
        /// Download exported users
        /// </summary>
        /// <remarks>Endpoint for getting export status of requests.</remarks>
        /// <param name="id"></param>
        /// <example>8754b7cb-d0fc-4499-8a1a-ebfb721cf0fc</example>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("export/{id:Guid}/download")]
        public async Task<IActionResult> DownloadAsync([FromRoute] Guid id)
        {
            var file = await _exportService.DownloadRequestsExportAsync(id);

            return File(file.Contents, file.ContentType, file.Name);
        }

        #region Private methods


        private async Task UpdateUserRolesAsync(UpdateUserViewModel model, ApplicationUser user)
        {
            _logger.LogInformation("determine is the user role has changed or not");

            var updatedRoles = model.Roles;
            var currentRoles = await _userManager.GetRolesAsync(user);

            var rolesToRemove = currentRoles.Except(updatedRoles).ToList();
            if (rolesToRemove.Any())
            {
                _logger.LogInformation("removing the user from roles");
                await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            }

            var rolesToAdd = updatedRoles.Except(currentRoles).ToList();
            if (rolesToAdd.Any())
            {
                _logger.LogInformation("adding user to the new roles");
                await _userManager.AddToRolesAsync(user, rolesToAdd);
            }
        }

        private async Task UpdateUserClaimsAsync(UpdateUserViewModel model, ApplicationUser user)
        {
            _logger.LogInformation("proceed to update the user claims");
            var oldClaims = await _userManager.GetClaimsAsync(user);

            if (oldClaims.Any())
            {
                _logger.LogInformation($"remove old user claims {oldClaims.ToJson()}");
                await _userManager.RemoveClaimsAsync(user, oldClaims);
            }

            var newClaims = model.Claims.Select(c => new Claim(c.Key, c.Value)).ToList();
            newClaims.Add(new Claim("name", model.Fullname));
            
            if (newClaims.Any())
            {
                _logger.LogInformation($"add new user claims {newClaims.ToJson()}");
                await _userManager.AddClaimsAsync(user, newClaims);
            }
        }

        private ExportStatusResponse PrepareStatusResponse(ExportStatusResponse request, ActionContext actionContext)
        {
            var urlHelper = new UrlHelper(actionContext);

            switch (request.Status)
            {
                case ExportStatus.Complete:
                    request.RequestUri = urlHelper.Action("download", values: new { id = request.Id });
                    break;
                case ExportStatus.Processing:
                    request.RequestUri = urlHelper.Action("GetExportStatus", values: new { id = request.Id });
                    break;
            }
            return request;
        }

        #endregion
    }
}