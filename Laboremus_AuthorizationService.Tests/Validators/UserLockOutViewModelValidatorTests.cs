using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Laboremus_AuthorizationService.Models;
using Laboremus_AuthorizationService.Validators;
using Xunit;

namespace Laboremus_AuthorizationService.Tests.Validators
{
    public class UserLockOutViewModelValidatorTests
    {
        private readonly UserLockOutViewModelValidator _validator;

        public UserLockOutViewModelValidatorTests()
        {
            _validator = new UserLockOutViewModelValidator();
        }

        [Fact]
        public void Validator_ShouldHaveErrorWhenUserLockOutIsNotIncluded()
        {
            var model = new UserLockOutViewModel
            {
            };

            var result = _validator.Validate(model);

            Assert.False(result.IsValid);
            Assert.Contains("lockedOut", result.Errors.Select(x => x.PropertyName));
            Assert.Contains("UserLockOutViewModel.lockedOut.NullOrEmpty", result.Errors.Select(x => x.ErrorCode));
        }
    }
}
