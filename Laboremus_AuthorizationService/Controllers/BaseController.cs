using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Laboremus_AuthorizationService.Controllers
{
    /// <inheritdoc />
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.BadGateway)]
    [ProducesResponseType((int)HttpStatusCode.Forbidden)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public class BaseController : Controller
    {
    }


    [Consumes("application/json")]
    [Produces("application/json")]
    //[ApiExplorerSettings(IgnoreApi = true)]
    [ProducesResponseType(typeof(BadRequestObjectResult), 500)]
    [ProducesResponseType(typeof(NotFoundResult), 404)]
    [ProducesResponseType(typeof(ForbidResult), 403)]
    [ProducesResponseType(typeof(UnauthorizedResult), 401)]
    //[Authorize(Roles = "administrator", AuthenticationSchemes = "Bearer")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class BaseAdminApiController : Controller
    {
    }
}
