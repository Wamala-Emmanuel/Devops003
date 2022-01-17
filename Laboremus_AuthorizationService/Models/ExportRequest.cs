using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Laboremus_AuthorizationService.Models
{
    public class ExportRequest
    {
        [Key]
        public Guid Id { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ExportStatus GenerationStatus { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public string Request { get; set; }

        public DateTime? DownloadedOn { get; set; }

        public string FileName { get; set; }

        public bool IsDeleted { get; set; }
    }
}
