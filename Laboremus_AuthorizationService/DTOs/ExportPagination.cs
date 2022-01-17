using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Laboremus_AuthorizationService.DTOs
{
    public class ExportPagination
    {
        /// <summary>
        /// Page number
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// Number of requests returned per page
        /// </summary>
        public int ItemsPerPage { get; set; }

        /// <summary>
        /// Total number of requests
        /// </summary>
        public int TotalItems { get; set; }

    }
}
