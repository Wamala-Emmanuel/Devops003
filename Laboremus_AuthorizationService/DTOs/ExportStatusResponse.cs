using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Laboremus_AuthorizationService.Models
{
    public class ExportStatusResponse : PendingResponse
    {
        public ExportStatusResponse()
        {
        }

        public ExportStatusResponse(Guid requestId, ExportStatus status)
        {
            Id = requestId;
            Status = status;
        }

        /// <summary>
        /// Status of the request
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ExportStatus Status { get; set; }

    }
}
