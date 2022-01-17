using System;
using Microsoft.AspNetCore.Mvc;

namespace Laboremus_AuthorizationService.Models
{
    public class PendingResponse : ActionResult
    {
        /// <summary>
        /// Unique Id of the request
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The Url to find the request
        /// </summary>
        public string RequestUri { get; set; }
    }
}
