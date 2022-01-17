using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace GatewayService.Controllers
    
{
    [Authorize]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [SwaggerResponse(
            StatusCodes.Status400BadRequest,
            "The request data is invalid",
            typeof(DTOs.ErrorResponse))]
    [SwaggerResponse(
            StatusCodes.Status401Unauthorized,
            "Not authorized to access the endpoint",
            typeof(DTOs.ErrorResponse))]
    [SwaggerResponse(
            StatusCodes.Status403Forbidden,
            "Refuse to authorize access to the endpoint",
            typeof(DTOs.ErrorResponse))]
    [SwaggerResponse(
            StatusCodes.Status404NotFound,
            "Request does not exist",
            typeof(DTOs.ErrorResponse))]
    [SwaggerResponse(
            StatusCodes.Status500InternalServerError,
            "The server encountered an unexpected error",
            typeof(DTOs.ErrorResponse))]
    public class BaseController : ControllerBase
    {
    }
}