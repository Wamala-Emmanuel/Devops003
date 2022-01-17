// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AuthServicePluginBase;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Laboremus_AuthorizationService.Core.Extensions;
using Laboremus_AuthorizationService.Models;
using Laboremus_AuthorizationService.Models.AccountViewModels;
using Laboremus_AuthorizationService.Services;
using Laboremus_AuthorizationService.Services.Claims;
using Laboremus_AuthorizationService.Services.EmailSender;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PluginBase;

namespace Laboremus_AuthorizationService.Frontend.Account
{
    /// <inheritdoc />
    [SecurityHeaders]
    [Route("account")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IEventService _events;
        private readonly ILogger<AccountController> _logger;
        private readonly IConfiguration _configuration;
        private readonly ICustomClaimsService _customClaimsService;
        private List<PlugIn> _pluginsFromConfiguration;

        /// <inheritdoc />
        public AccountController(UserManager<ApplicationUser> userManager,
                                SignInManager<ApplicationUser> signInManager,
                                IIdentityServerInteractionService interaction,
                                IClientStore clientStore,
                                IAuthenticationSchemeProvider schemeProvider,
                                IEventService events,
                                ILogger<AccountController> logger,
                                IConfiguration configuration,
                                ICustomClaimsService customClaimsService,
                                RoleManager<IdentityRole> roleManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _interaction = interaction;
            _clientStore = clientStore;
            _schemeProvider = schemeProvider;
            _events = events;
            _logger = logger;
            _configuration = configuration;
            _customClaimsService = customClaimsService;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _pluginsFromConfiguration = _configuration.GetSection("Plugins").Get<List<PlugIn>>();
        }

        /// <summary>
        /// Show registration page
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet("register")]
        public IActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// Handle postback for user registration
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.Username);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = model.Username
                    };

                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        result = await _userManager.AddClaimsAsync(user, new Claim[]
                        {
                            new Claim(JwtClaimTypes.Name, $"{model.FirstName} {model.LastName}"),
                            new Claim(JwtClaimTypes.GivenName, $"{model.FirstName}"),
                            new Claim(JwtClaimTypes.FamilyName, $"{model.LastName}"),
                            new Claim(JwtClaimTypes.Email, $"{model.Email}"),
                            new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                            new Claim(JwtClaimTypes.PhoneNumber, $"{model.Telephone}"),
                        });

                        // _userManager.AddToRoleAsync()

                        if (!result.Succeeded)
                        {
                            ViewData["Errors"] = true;

                            ModelState.AddModelError("", result.Errors.First().Description);
                            return View(model);
                        }

                    }
                    else
                    {
                        ViewData["Errors"] = true;
                        ModelState.AddModelError("", result.Errors.First().Description);
                        return View(model);
                    }
                }
                else
                {

                }

                return View();
            }
            else
            {
                return View(model);
            }
        }

        /// <summary>
        /// Display all users
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet("users")]
        public async Task<IActionResult> Users()
        {
            var model = new List<UserViewModel>();

            var users = await _userManager.Users.ToListAsync();
            foreach (var user in users)
            {
                // get user claims
                var claims = await _userManager.GetClaimsAsync(user);

                var u = new UserViewModel
                {
                    Username = user.UserName,
                    Id = user.Id
                };

                foreach (var claim in claims)
                {
                    switch (claim.Type)
                    {
                        case "name":
                            u.Name = claim.Value;
                            break;
                        case "email":
                            u.Email = claim.Value;
                            break;
                        case "phone_number":
                            u.Telephone = claim.Value;
                            break;
                        case "role":
                            u.Role = claim.Value;
                            break;
                        default:
                            break;
                    }
                }

                model.Add(u);
            }

            return View(model);
        }

        /// <summary>
        /// Show login page
        /// </summary>
        [HttpGet("login")]
        public async Task<IActionResult> Login(string returnUrl)
        {
            // build a model so we know what to show on the login page
            var vm = await BuildLoginViewModelAsync(returnUrl);
            //vm.EnableLocalLogin = _configuration.OnPremiseAdEnabled();

            if (vm.IsExternalLoginOnly)
            {
                // we only have one option for logging in and it's an external provider
                return await ExternalLogin(vm.ExternalLoginScheme, returnUrl);
            }

            return View(vm);
        }

        /// <summary>
        /// Handle postback from username/password login
        /// </summary>
        [HttpPost("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginInputModel model, string button)
        {
            if (button != "login")
            {
                // the user clicked the "cancel" button
                var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);
                if (context != null)
                {
                    // if the user cancels, send a result back into IdentityServer as if they 
                    // denied the consent (even if this client does not require consent).
                    // this will send back an access denied OIDC error response to the client.
                    await _interaction.GrantConsentAsync(context, ConsentResponse.Denied);

                    // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                    return Redirect(model.ReturnUrl);
                }
                else
                {
                    // since we don't have a valid context, then we just go back to the home page
                    return Redirect("~/");
                }
            }

            if (ModelState.IsValid)
            {
                ApplicationUser user = null;

                if (_configuration.OnPremiseAdEnabled())
                {
                    user = await OnPremiseAdLogin(model);
                }

                else
                {
                    //find user by username
                    user = await _userManager.FindByNameAsync(model.Username);

                    // check if user is locked out
                    if (user != null && user.LockoutEnabled)
                    {
                        await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "Locked out account"));
                        _logger.LogError("User account {0} is LockedOut.", model.Username);

                        // adding locked out error message to ModelState
                        ModelState.AddModelError(string.Empty, AccountOptions.LockedOutErrorMessage);

                        var lockedOutModel = await BuildLoginViewModelAsync(model);
                        return View(lockedOutModel);
                    }
                    else
                    {
                        var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberLogin, lockoutOnFailure: true);
                        
                        if (!result.Succeeded)
                        {
                            ModelState.AddModelError(string.Empty, AccountOptions.InvalidCredentialsErrorMessage);

                            var viewModel = await BuildLoginViewModelAsync(model);
                            return View(viewModel);
                        }
                        user = await _userManager.FindByNameAsync(model.Username);

                    }
                }

                if (user != null)
                {
                    await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id, user.UserName));

                    // make sure the returnUrl is still valid, and if so redirect back to authorize endpoint or a local page
                    // the IsLocalUrl check is only necessary if you want to support additional local pages, otherwise IsValidReturnUrl is more strict
                    if (_interaction.IsValidReturnUrl(model.ReturnUrl) || Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }

                    return Redirect("~/");
                }

                await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "invalid credentials"));
                _logger.LogError($"Invalid credentials for {model.Username}");

                ModelState.AddModelError(string.Empty, AccountOptions.InvalidCredentialsErrorMessage);
            }

            // something went wrong, show form with error
            var vm = await BuildLoginViewModelAsync(model);
            return View(vm);
        }

        /// <summary>
        /// initiate roundtrip to external authentication provider
        /// </summary>
        [HttpGet("external/login")]
        public async Task<IActionResult> ExternalLogin(string provider, string returnUrl)
        {
            if (AccountOptions.WindowsAuthenticationSchemeName == provider)
            {
                // windows authentication needs special handling
                return await ProcessWindowsLoginAsync(returnUrl);
            }
            else
            {
                // start challenge and roundtrip the return URL and 
                var props = new AuthenticationProperties()
                {
                    RedirectUri = Url.Action("ExternalLoginCallback"),
                    Items =
                    {
                        { "returnUrl", returnUrl },
                        { "scheme", provider },
                    }
                };

                return Challenge(props, provider);
            }
        }

        /// <summary>
        /// Post processing of external authentication
        /// </summary>
        [HttpGet("login/external/callback")]
        public async Task<IActionResult> ExternalLoginCallback()
        {
            // read external identity from the temporary cookie
            var result = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
            if (result?.Succeeded != true)
            {
                throw new Exception($"External authentication error: {result?.Failure.Message}", result?.Failure);
            }
            
            _logger.LogInformation("lookup user and external provider info");
            var (user, provider) = await FindUserFromExternalProviderAsync(result);

            var claims = SanitizeClaims(result);
            _logger.LogInformation($"{claims.Count} user claims...");
            claims.Select(s => new { s.Type, s.Value }).ToList()
                .ForEach(q => _logger.LogInformation($"Type: {q.Type}, Value: {q.Value}"));

            if (user == null)
            {
                _logger.LogInformation("add the user to the database");
                user = await AutoProvisionUserAsync(provider, claims);
            }

            // Append static claims to the user
            // claims.AddRange(StaticClaims());

            // Run plugins for user claims
            // await RunPlugins(claims);

            await ProcessClaims(user, claims);

            // this allows us to collect any additonal claims or properties
            // for the specific protocols used and store them in the local auth cookie.
            // this is typically used to store data needed for signout from those protocols.

            var additionalLocalClaims = new List<Claim>();

            
            var localSignInProps = new AuthenticationProperties();
            ProcessLoginCallbackForOidc(result, additionalLocalClaims, localSignInProps);

            // ProcessLoginCallbackForWsFed(result, additionalLocalClaims, localSignInProps);
            // ProcessLoginCallbackForSaml2p(result, additionalLocalClaims, localSignInProps);

            await ProcessRoles(claims, user);

            _logger.LogInformation("issue authentication cookie for user");
            // we must issue the cookie manually, and can't use the SignInManager because
            // it doesn't expose an API to issue additional claims from the login workflow
            var principal = await _signInManager.CreateUserPrincipalAsync(user);

            // TODO
            // add claims to access token
            // representation_rights

            additionalLocalClaims.AddRange(principal.Claims);


            var name = principal.Claims.FirstOrDefault(s => s.Type == JwtClaimTypes.Name)?.Value ?? user.Id;
            await _events.RaiseAsync(new UserLoginSuccessEvent(provider, user.Id, user.Id, name));

            _logger.LogInformation("sign in the user");
            await HttpContext.SignInAsync(user.Id, name, provider, localSignInProps, additionalLocalClaims.ToArray());

            _logger.LogInformation("delete temporary cookie used during external authentication");
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            _logger.LogInformation("validate return URL and redirect back to authorization endpoint or a local page");
            var returnUrl = result.Properties.Items["returnUrl"];
            if (_interaction.IsValidReturnUrl(returnUrl) || Url.IsLocalUrl(returnUrl))
            {
                _logger.LogInformation("Redirect user to " + returnUrl);
                return Redirect(returnUrl);
            }

            _logger.LogInformation("Redirect user to the root of the auth service (~/)");
            return Redirect("~/");
        }

        /// <summary>
        /// display the forgot password form
        /// </summary>
        /// <returns></returns>
        [HttpGet("ForgotPassword")]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        /// <summary>
        /// handle Post forgot password form data
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("ForgotPassword")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPasswordAsync(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                //ViewBag.Email = model.Email;
                var confirmModel = new ForgotPasswordConfirmationViewModel { Email = model.Email };

                var user = await _userManager.FindByNameAsync(model.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View(nameof(ForgotPasswordConfirmation), confirmModel);
                }

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

                _logger.LogInformation("Tried to send reset password email for {0}.", model.Email);
                
                return View(nameof(ForgotPasswordConfirmation), confirmModel);
            }

            return View();
        }

        /// <summary>
        /// Show logout page
        /// </summary>
        [HttpGet("logout")]
        public async Task<IActionResult> Logout(string logoutId)
        {
            // build a model so the logout page knows what to display
            var vm = await BuildLogoutViewModelAsync(logoutId);

            if (vm.ShowLogoutPrompt == false)
            {
                // if the request for logout was properly authenticated from IdentityServer, then
                // we don't need to show the prompt and can just log the user out directly.
                return await Logout(vm);
            }

            return View(vm);
        }

        /// <summary>
        /// Handle logout page postback
        /// </summary>
        [HttpPost("logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(LogoutInputModel model)
        {
            // build a model so the logged out page knows what to display
            var vm = await BuildLoggedOutViewModelAsync(model.LogoutId);

            if (User?.Identity.IsAuthenticated == true)
            {
                // delete local authentication cookie;
                await HttpContext.SignOutAsync(IdentityServerConstants.DefaultCookieAuthenticationScheme);

                await _signInManager.SignOutAsync();
                
                // raise the logout event
                await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
            }

            // check if we need to trigger sign-out at an upstream identity provider
            if (vm.TriggerExternalSignout)
            {
                // build a return URL so the upstream provider will redirect back
                // to us after the user has logged out. this allows us to then
                // complete our single sign-out processing.
                string url = Url.Action("Logout", new { logoutId = vm.LogoutId });

                // this triggers a redirect to the external provider for sign-out
                return SignOut(new AuthenticationProperties { RedirectUri = url }, vm.ExternalAuthenticationScheme);
            }

            if (vm.AutomaticRedirectAfterSignOut && !string.IsNullOrEmpty(vm.PostLogoutRedirectUri))
            {
                return Redirect(vm.PostLogoutRedirectUri);
            }

            // delete local authentication cookie for direct AuthService Login
            if (vm.AutomaticRedirectAfterSignOut && string.IsNullOrEmpty(vm.PostLogoutRedirectUri))
            {
                HttpContext.Response.Cookies.Delete(".AspNetCore.Identity.Application");
            }
            return View("LoggedOut", vm);
        }

        /// <summary>
        /// Handle forgot password confirmation
        /// </summary>
        /// <returns></returns>
        [HttpGet("ForgotPasswordConfirmation")]
        public IActionResult ForgotPasswordConfirmation(ForgotPasswordConfirmationViewModel model)
        {
            return View(model);
        }

        /// <summary>
        /// Display reset password form
        /// </summary>
        /// <returns></returns>
        [HttpGet("ResetPassword")]
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
        /// handle reset password
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("ResetPassword")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.Email);
                if (user == null)
                {
                    return RedirectToAction(nameof(ResetPasswordConfirmation));
                }

                // decode code sent
                var decodedCodeBytes = WebEncoders.Base64UrlDecode(model.Code);
                var codeSent = Encoding.UTF8.GetString(decodedCodeBytes);
                
                var resetPassResult = await _userManager.ResetPasswordAsync(user, codeSent, model.Password);
                if (resetPassResult.Succeeded)
                {
                    return RedirectToAction(nameof(ResetPasswordConfirmation));
                }

                foreach (var error in resetPassResult.Errors)
                {
                    ModelState.TryAddModelError(error.Code, error.Description);
                }
                return View();
            }

            return View(model);
        }

        /// <summary>
        /// handle reset password confirmation
        /// </summary>
        /// <returns></returns>
        [HttpGet("ResetPasswordConfirmation")]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        #region APIs

        /*****************************************/
        /* helper APIs for the AccountController */
        /*****************************************/

        private async Task<ApplicationUser> OnPremiseAdLogin(LoginInputModel model)
        {
            ApplicationUser user = null;
            var context = new PrincipalContext(ContextType.Domain, _configuration.GetAdDomain());

            try
            {
                var userPrincipal =
                    UserPrincipal.FindByIdentity(context, _configuration.GetAdIdentityType(), model.Username.ToLower());

                if (userPrincipal != null)
                {
                    var isValidPassword =
                        context.ValidateCredentials(model.Username, model.Password);

                    if (isValidPassword)
                    {
                        // var groups = userPrincipal.GetAuthorizationGroups().ToList();

                        userPrincipal.EmailAddress = userPrincipal.EmailAddress ?? userPrincipal.UserPrincipalName;
                        user = await _userManager.FindByEmailAsync(userPrincipal.EmailAddress);

                        var claims = new List<Claim>();
                        if (user == null)
                        {
                            var applicationUser = new ApplicationUser
                            {
                                Email = userPrincipal.EmailAddress,
                                UserName = model.Username,
                                Id = userPrincipal.Guid.ToString()
                            };

                            var result = await _userManager.CreateAsync(applicationUser);
                            if (result.Succeeded)
                            {
                                user = await _userManager.FindByEmailAsync(userPrincipal.EmailAddress);

                                claims.AddRange(new List<Claim>
                                {
                                    new Claim(JwtClaimTypes.Name, $"{userPrincipal.Name}"),
                                    new Claim(JwtClaimTypes.GivenName, $"{userPrincipal.GivenName}"),
                                    new Claim(JwtClaimTypes.FamilyName, $"{userPrincipal.Surname}"),
                                    new Claim(JwtClaimTypes.Email, $"{userPrincipal.EmailAddress}"),
                                });
                            }
                        }

                        //Append static claims to the user
                        var staticClaims = StaticClaims();
                        await ProcessClaims(user, staticClaims);
                        await _signInManager.SignInAsync(user, true);
                    }
                    else
                    {
                        await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "invalid password"));
                        _logger.LogError($"Invalid password for {model.Username}");
                        ModelState.AddModelError("", AccountOptions.InvalidCredentialsErrorMessage);
                    }
                }
                else
                {
                    await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "invalid username"));
                    _logger.LogError($"UserPrincipal not found for {model.Username} (Invalid username)");
                    ModelState.AddModelError("", AccountOptions.InvalidCredentialsErrorMessage);
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"Unexpected error on AD login: {exception.Message}");
                ModelState.AddModelError("", exception.Message);
            }

            return user;
        }

        private async Task ProcessRoles(List<Claim> claims, ApplicationUser user)
        {
            _logger.LogInformation("read the user role(s) from the external identity provider");

            // check if there is a role from Azure AD
            var adRole = claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value;

            if (adRole != null) // AD role always takes precedence
            {
                var role = adRole.ToUpper();
                // is this role already assigned to the user in Auth Service?
                var roleExists = await _roleManager.RoleExistsAsync(role);

                // if it is not, then add it and assign it to the user
                if (!roleExists)
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                    await _userManager.AddToRoleAsync(user, role);
                }
            }

            // check if there are any static roles
            var staticRole = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Role)?.Value.ToUpper();

            if (staticRole != null)
            {
                _logger.LogInformation($"check if the role {staticRole} exists in the database");
                var roleExists = await _roleManager.RoleExistsAsync(staticRole);

                if (!roleExists)
                {
                    _logger.LogInformation($"role {staticRole} does not exist");
                    _logger.LogInformation($"now add it as a new role");
                    await _roleManager.CreateAsync(new IdentityRole(staticRole));
                }

                var systemRoles = _configuration.GetSection("SystemRoles").Get<List<string>>();
                var isInRole = await _userManager.IsInRoleAsync(user, staticRole);
                if (!isInRole)
                {
                    _logger.LogInformation($"assign role {staticRole} to user {user.UserName}");
                    await _userManager.AddToRoleAsync(user, staticRole);
                }

                _logger.LogInformation("remove user from other roles which are not part of the system roles list");
                // it should be possible for someone to be part of multiple roles
                var userRoles = await _userManager.GetRolesAsync(user);
                foreach (var userRole in userRoles)
                {
                    if (userRole != staticRole && !systemRoles.Contains(userRole))
                    {
                        await _userManager.RemoveFromRoleAsync(user, userRole);
                    }
                }
            }
        }

        /// <summary>
        /// Some claim types are the same e.g. JwtClaimTypes.Role and JwtClaimTypes.Role
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private static List<Claim> SanitizeClaims(AuthenticateResult result)
        {
            var rawClaims = result.Principal.Claims.ToList();

            var claims = new List<Claim>();
            foreach (var claim in rawClaims)
            {
                if (claim.Type == ClaimTypes.Role || claim.Type == JwtClaimTypes.Role)
                {
                    claims.Add(new Claim(JwtClaimTypes.Role, claim.Value));
                }
                else
                {
                    claims.Add(claim);
                }
            }

            return claims;
        }

        private void ConfigurePlugin(IPlugin plugin, string pluginName)
        {
            var configurationData = _pluginsFromConfiguration.Where(x => x.Name == pluginName).Select(c => c.ConfigurationData).FirstOrDefault();
            plugin.Configure(configurationData);
        }

        private async Task RunPlugins(List<Claim> claims)
        {
            var rolesInfoResponse = string.Empty;

            //take context services			
            var contextServices = HttpContext.RequestServices;

            //take IRolesInfoPlugin 
            //if it's loaded use it
            var rolesInfoPlugin = (IRolesInfoPlugin)contextServices.GetService(typeof(IRolesInfoPlugin));
            if (rolesInfoPlugin != null)
            {
                ConfigurePlugin(rolesInfoPlugin, typeof(IRolesInfoPlugin).Name);

                rolesInfoResponse = await rolesInfoPlugin.GetRoleInfoObject(claims);

                _logger.LogInformation($"Roles response info: {rolesInfoResponse}");
            }

            //take IClaimsAdjusterPlugin 
            //if it's loaded use it
            var claimsAdjusterPlugin = (IClaimsAdjusterPlugin)contextServices.GetService(typeof(IClaimsAdjusterPlugin));
            if (claimsAdjusterPlugin != null)
            {
                ConfigurePlugin(claimsAdjusterPlugin, typeof(IClaimsAdjusterPlugin).Name);
                claimsAdjusterPlugin.AdjustAccessTokenClaims(claims, rolesInfoResponse);
            }

            //take IIdentityTokenPlugin 
            //if it's loaded use it
            var identityTokenPlugin = (IIdentityTokenPlugin)contextServices.GetService(typeof(IIdentityTokenPlugin));
            if (identityTokenPlugin != null)
            {
                ConfigurePlugin(identityTokenPlugin, typeof(IIdentityTokenPlugin).Name);
                await identityTokenPlugin.AdjustIdentityTokenClaims(claims);
            }
        }

        private List<Claim> StaticClaims()
        {
            var userClaims = new List<Claim>();
            var perClientUserClaims = _configuration.GetSection("StaticUserClaims").Get<Dictionary<string, Dictionary<string, string>>>();
            if (perClientUserClaims.Any())
            {
                foreach (var clientUserClaim in perClientUserClaims)
                {
                    var claims = clientUserClaim.Value;
                    userClaims.AddRange(claims.Select(x => new Claim(x.Key, x.Value)));
                }
            }
            return userClaims;
        }

        private async Task ProcessClaims(ApplicationUser user, IEnumerable<Claim> claims)
        {
            var existingClaims = await _userManager.GetClaimsAsync(user);
            await RemoveDuplicateClaims(user, existingClaims);

            var newClaims = new List<Claim>();

            var ignoredClaims = new List<string>
            {
                "aio", "uti"
            };

            foreach (var claim in claims)
            {
                // check if this claim needs to be ignored
                if (ignoredClaims.Contains(claim.Type.ToLower())) continue;

                if (claim.Type == JwtClaimTypes.Role)
                {
                    var role = claim.Value.ToUpper();
                    if (await _roleManager.RoleExistsAsync(role) == false)
                    {
                        await _roleManager.CreateAsync(new IdentityRole(role));
                    }

                    var isInRole = await _userManager.IsInRoleAsync(user, role);
                    if (!isInRole)
                    {
                        await _userManager.AddToRoleAsync(user, role);
                    }
                    continue;
                }
                
                var oldClaim = existingClaims.FirstOrDefault(x => x.Type == claim.Type);
                if (oldClaim != null)
                {
                    if (oldClaim.Value == claim.Value) continue;
                    // if the value has changed, update it
                    await _userManager.ReplaceClaimAsync(user, oldClaim, claim);
                    continue;
                }

                newClaims.Add(claim);
            }

            await _userManager.AddClaimsAsync(user, newClaims);
        }

        private async Task RemoveDuplicateClaims(ApplicationUser user, IEnumerable<Claim> claims)
        {
            var duplicates = claims
                .GroupBy(x => new { x.Type, x.Value})
                .Where(grp => grp.Count() > 1)
                .Select(grp => new Claim(grp.Key.Type, grp.Key.Value));

            foreach (var duplicate in duplicates)
            {
                await _userManager.RemoveClaimAsync(user, duplicate); // this actually removes all rows from AspNetUserClaims table for the given claim type and value
                await _userManager.AddClaimAsync(user, duplicate); //because previous command removes all rows we need to add one back
            }
        }

        private async Task<LoginViewModel> BuildLoginViewModelAsync(string returnUrl)
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (context?.IdP != null)
            {
                // this is meant to short circuit the UI and only trigger the one external IdP
                return new LoginViewModel
                {
                    EnableLocalLogin = false,
                    ReturnUrl = returnUrl,
                    Username = context?.LoginHint,
                    ExternalProviders = new ExternalProvider[] { new ExternalProvider { AuthenticationScheme = context.IdP } },
                    AzureAdB2CProviders = new ExternalProvider[] { new ExternalProvider { AuthenticationScheme = context?.IdP, DisplayName = "Azure AD B2C" }, }
                };
            }

            var schemes = await _schemeProvider.GetAllSchemesAsync();

            var providers = schemes
                .Where(x => x.DisplayName != null ||
                            (x.Name.Equals(AccountOptions.WindowsAuthenticationSchemeName, StringComparison.OrdinalIgnoreCase))
                )
                .Select(x => new ExternalProvider
                {
                    DisplayName = x.DisplayName,
                    AuthenticationScheme = x.Name
                }).ToList();

            var allowLocal = true;
            if (context?.ClientId != null)
            {
                var client = await _clientStore.FindEnabledClientByIdAsync(context.ClientId);
                if (client != null)
                {
                    allowLocal = client.EnableLocalLogin;

                    if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
                    {
                        providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
                    }
                }
            }

            return new LoginViewModel
            {
                AllowRememberLogin = false, // AccountOptions.AllowRememberLogin,
                EnableLocalLogin = allowLocal && AccountOptions.AllowLocalLogin,
                ReturnUrl = returnUrl,
                Username = context?.LoginHint,
                ExternalProviders = providers.ToArray()
            };
        }

        private async Task<LoginViewModel> BuildLoginViewModelAsync(LoginInputModel model)
        {
            var vm = await BuildLoginViewModelAsync(model.ReturnUrl);
            vm.Username = model.Username;
            vm.RememberLogin = model.RememberLogin;
            return vm;
        }

        private async Task<LogoutViewModel> BuildLogoutViewModelAsync(string logoutId)
        {
            var vm = new LogoutViewModel { LogoutId = logoutId, ShowLogoutPrompt = AccountOptions.ShowLogoutPrompt };

            if (User?.Identity.IsAuthenticated != true)
            {
                // if the user is not authenticated, then just show logged out page
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            var context = await _interaction.GetLogoutContextAsync(logoutId);
            if (context?.ShowSignoutPrompt == false)
            {
                // it's safe to automatically sign-out
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            // show the logout prompt. this prevents attacks where the user
            // is automatically signed out by another malicious web page.
            return vm;
        }

        private async Task<LoggedOutViewModel> BuildLoggedOutViewModelAsync(string logoutId)
        {
            // get context information (client name, post logout redirect URI and iframe for federated signout)
            var logout = await _interaction.GetLogoutContextAsync(logoutId);

            var vm = new LoggedOutViewModel
            {
                AutomaticRedirectAfterSignOut = AccountOptions.AutomaticRedirectAfterSignOut,
                PostLogoutRedirectUri = logout?.PostLogoutRedirectUri,
                ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout?.ClientName,
                SignOutIframeUrl = logout?.SignOutIFrameUrl,
                LogoutId = logoutId
            };

            if (User?.Identity.IsAuthenticated == true)
            {
                var idp = User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
                if (idp != null && idp != IdentityServer4.IdentityServerConstants.LocalIdentityProvider)
                {
                    var providerSupportsSignout = await HttpContext.GetSchemeSupportsSignOutAsync(idp);
                    if (providerSupportsSignout)
                    {
                        if (vm.LogoutId == null)
                        {
                            // if there's no current logout context, we need to create one
                            // this captures necessary info from the current logged in user
                            // before we signout and redirect away to the external IdP for signout
                            vm.LogoutId = await _interaction.CreateLogoutContextAsync();
                        }

                        vm.ExternalAuthenticationScheme = idp;
                    }
                }
            }

            return vm;
        }

        private async Task<IActionResult> ProcessWindowsLoginAsync(string returnUrl)
        {
            // see if windows auth has already been requested and succeeded
            var result = await HttpContext.AuthenticateAsync(AccountOptions.WindowsAuthenticationSchemeName);
            if (result?.Principal is WindowsPrincipal wp)
            {
                // we will issue the external cookie and then redirect the
                // user back to the external callback, in essence, tresting windows
                // auth the same as any other external authentication mechanism
                var props = new AuthenticationProperties()
                {
                    RedirectUri = Url.Action("ExternalLoginCallback"),
                    Items =
                    {
                        { "returnUrl", returnUrl },
                        { "scheme", AccountOptions.WindowsAuthenticationSchemeName },
                    }
                };

                var id = new ClaimsIdentity(AccountOptions.WindowsAuthenticationSchemeName);
                id.AddClaim(new Claim(JwtClaimTypes.Subject, wp.Identity.Name));
                id.AddClaim(new Claim(JwtClaimTypes.Name, wp.Identity.Name));

                // add the groups as claims -- be careful if the number of groups is too large
                if (AccountOptions.IncludeWindowsGroups)
                {
                    var wi = wp.Identity as WindowsIdentity;
                    var groups = wi.Groups.Translate(typeof(NTAccount));
                    var roles = groups.Select(x => new Claim(JwtClaimTypes.Role, x.Value));
                    id.AddClaims(roles);
                }

                await HttpContext.SignInAsync(
                    IdentityServer4.IdentityServerConstants.ExternalCookieAuthenticationScheme,
                    new ClaimsPrincipal(id),
                    props);
                return Redirect(props.RedirectUri);
            }
            else
            {
                // trigger windows auth
                // since windows auth don't support the redirect uri,
                // this URL is re-triggered when we call challenge
                return Challenge(AccountOptions.WindowsAuthenticationSchemeName);
            }
        }

        private async Task<(ApplicationUser user, string provider)>
            FindUserFromExternalProviderAsync(AuthenticateResult result)
        {
            var externalUser = result.Principal;

            // try to determine the unique id of the external user (issued by the provider)
            // the most common claim type for that are the sub claim and the NameIdentifier
            // depending on the external provider, some other claim type might be used

            // Object Identifier Claim - http://schemas.microsoft.com/identity/claims/objectidentifier
            const string objectIdentifier = "http://schemas.microsoft.com/identity/claims/objectidentifier";

            var userId = (externalUser.FindFirst(objectIdentifier) ??
                              externalUser.FindFirst(JwtClaimTypes.Subject) ??
                              externalUser.FindFirst(JwtClaimTypes.Id))?.Value;

            var email = (externalUser.FindFirst(ClaimTypes.Email) ??
                             externalUser.FindFirst(JwtClaimTypes.Email) ??
                             externalUser.FindFirst(ClaimTypes.Name) ??
                             externalUser.FindFirst(ClaimTypes.Upn))?.Value;

            // a valid email address should contain an "@" and a .
            email = email != null && email.IsValidEmailAddress() ? email : null;

            // remove the user id claim so we don't include it as an extra claim if/when we provision the user
            // var claims = externalUser.Claims.ToList();

            var provider = result.Properties.Items["scheme"];

            // find external user
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null && email != null)
            {
                user = await _userManager.FindByEmailAsync(email);
            }

            return (user, provider);
        }

        private async Task<ApplicationUser> AutoProvisionUserAsync(string provider, IReadOnlyCollection<Claim> claims)
        {
            _logger.LogInformation("create a list of claims to transfer into the user store");
            var filtered = new List<Claim>();

            var givenName = claims.FirstOrDefault(x => x.Type == ClaimTypes.GivenName)?.Value;
            var surname = claims.FirstOrDefault(x => x.Type == ClaimTypes.Surname)?.Value;

            if (givenName != null && surname != null)
            {
                filtered.Add(new Claim(JwtClaimTypes.Name, givenName + " " + surname));
            }

            else if (givenName != null)
            {
                filtered.Add(new Claim(JwtClaimTypes.Name, givenName));
            }
            else if (surname != null)
            {
                filtered.Add(new Claim(JwtClaimTypes.Name, surname));
            }

            // email
            var emailTypes = new[] { ClaimTypes.Upn, ClaimTypes.Name, ClaimTypes.Email, JwtClaimTypes.Email };
            var email = claims.FirstOrDefault(q => emailTypes.Any(e => q.Type == e))?.Value;

            if (email != null && email.IsValidEmailAddress())
            {
                filtered.Add(new Claim(JwtClaimTypes.Email, email));
            }

            // User Id
            var idTypes = new[]
            {
                "http://schemas.microsoft.com/identity/claims/objectidentifier",
                ClaimTypes.NameIdentifier,
                JwtClaimTypes.Id,
                JwtClaimTypes.Subject
            };

            var userId = claims.FirstOrDefault(x => idTypes.Any(i => x.Type == i))?.Value;

            try
            {
                _logger.LogInformation($"find user by user id: {userId}");
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("the user does not exist");
                    user = new ApplicationUser
                    {
                        UserName = email ?? Guid.NewGuid().ToString(),
                        Email = email,
                        Id = userId ?? Guid.NewGuid().ToString()
                    };

                    _logger.LogInformation($"create as a new user: {user.ToString()}");
                    var identityResult = await _userManager.CreateAsync(user);
                    if (!identityResult.Succeeded)
                    {
                        var errors = string.Join(",", identityResult.Errors.Select(s => s.Description).ToList());
                        _logger.LogError(errors);
                        throw new Exception(errors);
                    }

                    var extraClaims = await _customClaimsService.GetExtraClaimsAsync(user);
                    if (extraClaims?.Any() ?? false)
                    {
                        filtered.AddRange(extraClaims);
                    }

                    if (filtered.Any())
                    {

                        identityResult = await _userManager.AddClaimsAsync(user, filtered);
                        if (!identityResult.Succeeded)
                        {
                            var errors = string.Join(",", identityResult.Errors.Select(s => s.Description).ToList());
                            _logger.LogError(errors);
                            throw new Exception(errors);
                        }
                    }

                    identityResult = await _userManager.AddLoginAsync(user, new UserLoginInfo(provider, userId, provider));
                    if (!identityResult.Succeeded)
                    {
                        var errors = string.Join(",", identityResult.Errors.Select(s => s.Description).ToList());
                        _logger.LogError(errors);
                        throw new Exception(errors);
                    }
                }

                return user;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, exception.Message);
                throw new Exception(exception.Message, exception);
            }


        }


        private void ProcessLoginCallbackForOidc(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
        {
            // if the external system sent a session id claim, copy it over
            // so we can use it for single sign-out
            var sid = externalResult.Principal.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);
            if (sid != null)
            {
                localClaims.Add(new Claim(JwtClaimTypes.SessionId, sid.Value));
            }

            // if the external provider issued an id_token, we'll keep it for signout
            var idToken = externalResult.Properties.GetTokenValue("id_token");
            if (idToken != null)
            {
                _logger.LogInformation($"Id Token: {idToken}");
                localSignInProps.StoreTokens(new[] { new AuthenticationToken { Name = "id_token", Value = idToken } });
            }
        }

        private void ProcessLoginCallbackForWsFed(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
        {
        }

        private void ProcessLoginCallbackForSaml2p(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
        {
        }

        #endregion

    }
}