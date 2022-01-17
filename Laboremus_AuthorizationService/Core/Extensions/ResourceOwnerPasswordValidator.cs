using System.Threading.Tasks;
using IdentityServer4.Validation;
using Laboremus_AuthorizationService.Models;
using Microsoft.AspNetCore.Identity;

namespace Laboremus_AuthorizationService.Core.Extensions
{
    public class ResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ResourceOwnerPasswordValidator(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            var signIn = await _signInManager.PasswordSignInAsync(context.UserName, context.Password, true, false);
            if (signIn.Succeeded)
            {
                var subject = await _userManager.FindByNameAsync(context.UserName);
            }

            // throw new NotImplementedException();
        }
    }
}
