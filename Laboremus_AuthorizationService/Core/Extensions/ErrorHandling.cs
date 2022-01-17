#pragma warning disable CS1591 // Missing XML comment
using System;
using System.Data.SqlClient;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Laboremus_AuthorizationService.Core.Exceptions;
using Laboremus_AuthorizationService.Core.Helpers.Logger;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using Newtonsoft.Json;
using Serilog.Context;

namespace Laboremus_AuthorizationService.Core.Extensions
{
    public class ErrorHandling
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _config;

        public ErrorHandling(RequestDelegate next, IConfiguration config)
        {
            _next = next;
            _config = config;
        }

        public async Task Invoke(HttpContext context )
        {
            var correlationId = context.Request.Headers["X-Correlation-Id"].ToString();
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                try
                {
                    await _next(context);
                }
                catch (Exception ex)
                {
                    await HandleExceptionAsync(context, ex);
                }
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var code = HttpStatusCode.InternalServerError; // 500 if unexpected
            var errorMsg = exception.Message;
            //You can test as many exception types as you want/have
            if (exception is NotFoundException) { code = HttpStatusCode.NotFound; }

            if (exception is UnauthorizedAccessException)
            {
                code = HttpStatusCode.Unauthorized;
            }
            if (exception.Message.ToLower() == "unauthorized")
            {
                code = HttpStatusCode.Unauthorized;
            }
            if (exception is BadHttpRequestException) { code = HttpStatusCode.BadRequest; }
            else if (exception is ApplicationValidationException) code = HttpStatusCode.BadRequest;
            else if (exception is DbUpdateException || exception is SqlException || exception is AuthenticationException)
            {
                code = HttpStatusCode.BadRequest;
                if (exception.InnerException?.Message != null) // For any sqldb exception, return inner exception message
                    errorMsg = exception.InnerException.Message;
            }
            
            if (code == HttpStatusCode.Unauthorized)
            {
                await context.SignOutAsync();
                context.Response.Redirect("/user/login");
            }

            //Serialize error message
            var result = JsonConvert.SerializeObject(new { Code = code, Message = errorMsg, exception.StackTrace });
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;
            await context.Response.WriteAsync(result);

            // If the request is local, don't send the email. This is normally important when in debug mode,
            // you don't want to send emails to your self during development because there can be too many errors
            // if (context.Request.IsLocal()) return;

            // Error logging here
            var url = _config.GetSection("LogServer").Value;
            var userId = context.Request.Headers["UserId"].ToString();
            var organisationId = context.Request.Headers["OrganisationId"].ToString();

            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(organisationId))
            {
                await Task.Run(() => Logger.DoExceptionLogAsync(url, exception, ClientService.AuthService, userId, organisationId),
                    new CancellationToken());
            }

            var exceptionType = exception.GetType().Name;

            //Send an email only if an error is not a custom exception implementation, you don't want to get
            //emails for errors you have difined your self e.g Id can not be null
            if (exceptionType == "ApplicationValidationException" ||
                exceptionType == "NotFoundException" ||
                exceptionType == "DbUpdateException" ||
                exceptionType == "SqlException") return;

            //Send an email to the support group
            var emailSubject = exceptionType;
            var body = exception.ToString();
            // await _emailSender.SendEmailAsync("wilson@laboremus.no", emailSubject, body);

        }
    }
}
