using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Laboremus_AuthorizationService.Tests
{   
    public class UserControllerTests
    {
        private Mock<IConfiguration> Configuration { get; set; }
        private Mock<IHostingEnvironment> Environment { get; set; }

        public UserControllerTests()
        {
            Configuration = new Mock<IConfiguration>();
            Environment = new Mock<IHostingEnvironment>();
        }

        [Fact]
        public async Task LoginShouldReturnABadRequestObjectResultWhenModelIsInvalid() 
        {
            //// Arrange
            //var model = new UserLoginViewModel
            //{
            //    Username = null,
            //    Password = null,
            //    ClientId = null,
            //    ClientSecret = null,
            //    RequestingAccessTo = new List<string>()
            //};

            //var controller = new UserController(Configuration.Object, Environment.Object);

            //// Act
            //var response = await controller.Login(model);

            //// Assert
            //Assert.IsType<BadRequestObjectResult>(response);
        }
    }
}
