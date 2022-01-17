using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Laboremus_AuthorizationService.DTOs;
using Laboremus_AuthorizationService.Models;

namespace Laboremus_AuthorizationService.Validators
{
    public class UserExportRequestModelValidator : AbstractValidator<UserExportRequest>
    {
        public UserExportRequestModelValidator()
        {
            RuleForEach(r => r.Roles)
                .IsEnumName(typeof(UserRole), caseSensitive: false)
                .When(r => r.Roles != null);

            RuleFor(r => r.lockedOut)
                .NotNull()
                .WithErrorCode("UserLockOutViewModel.lockedOut.BooleanValueExpected")
                .When(r => r.lockedOut.HasValue );

        }
    }
}
