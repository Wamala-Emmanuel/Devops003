using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Laboremus_AuthorizationService.Models;

namespace Laboremus_AuthorizationService.Validators
{
    public class UserLockOutViewModelValidator : AbstractValidator<UserLockOutViewModel>
    {
        public UserLockOutViewModelValidator()
        {
            RuleFor(x => x.lockedOut)
                .NotEmpty()
                .WithErrorCode("UserLockOutViewModel.lockedOut.NullOrEmpty");
        }
    }
}
