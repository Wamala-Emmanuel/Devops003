using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Laboremus_AuthorizationService.DTOs;
using Laboremus_AuthorizationService.Validators;
using Xunit;

namespace Laboremus_AuthorizationService.Tests.Validators
{
    public class UserExportRequestModelValidatorTests
    {
        private readonly UserExportRequestModelValidator _validator;

        public UserExportRequestModelValidatorTests()
        {
            _validator = new UserExportRequestModelValidator();
        }


        [Fact]
        public void Validator_ShouldHaveErrorWhenUserRoleIsInvalid()
        {
            var model = new UserExportRequest
            {
                Roles = new List<string>
                {
                    "accountant",
                },
            };

            var result = _validator.Validate(model);

            Assert.False(result.IsValid);
            Assert.Contains("Roles[0]", result.Errors.Select(x => x.PropertyName));
            Assert.Contains("StringEnumValidator", result.Errors.Select(x => x.ErrorCode));
        }
    }
}
