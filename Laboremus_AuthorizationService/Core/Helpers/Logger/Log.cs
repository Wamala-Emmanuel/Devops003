using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;

namespace Laboremus_AuthorizationService.Core.Helpers.Logger
{
    public class Log
    {
        public LogType Type { get; set; }
        public string OrganisationId { get; set; }    

        public object MetaData { get; set; }   
    }

    public enum LogType
    {
        /// <summary>
        /// Add a new entry
        /// </summary>
        [Display(Name = "Add a new entry")]
        Add = 1,

        /// <summary>
        /// Modify an entr3
        /// </summary>
        Modify = 2,

        /// <summary>
        /// Delete an entry
        /// </summary>
        Delete = 3,

        /// <summary>
        /// Access a resource
        /// </summary>
        Access = 4,

        /// <summary>
        /// Throw an exception
        /// </summary>
        Exception = 5
    }

    public class LogResponse
    {
        public IEnumerable<ChangeLog> Changes { get; set; }
        public IEnumerable<AccessLog> AccessLogs { get; set; }      
        public IEnumerable<ExceptionLog> ExceptionLogs { get; set; }
    }

    public class AccessLog
    {
        public string IpAddress { get; set; }
        public string Service { get; set; }
        public string OrganisationId { get; set; }   
        public string UserId { get; set; }
        public Request Request { get; set; }
        public Response Response { get; set; }  
    }

    public class Request
    {
        public DateTime Date { get; set; }
        public string ContentType { get; set; }
        public HttpMethod Method { get; set; }
        public string Body { get; set; }
        public string Uri { get; set; } 
    }

    public class Response
    {
        public DateTime Date { get; set; }
        public int StatusCode { get; set; }
        public string Body { get; set; }
    }

    public class ExceptionLog
    {
        public DateTime Date { get; set; }  
        public string IpAddress { get; set; }
        public string Service { get; set; }
        public string OrganisationId { get; set; }  
        public string UserId { get; set; }
        public CustomException Exception { get; set; }  

    }

    public class CustomException
    {
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public string Type { get; set; }
        public InnerException InnerException { get; set; } 
    }

    public class InnerException
    {
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public string Type { get; set; }
    }
}
