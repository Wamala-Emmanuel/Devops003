using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Laboremus_AuthorizationService.DTOs
{
    public class UserExportRequest
    {
        /// <summary>
        /// user roles
        /// </summary>
        public ICollection<string> Roles { get; set; }

        /// <summary>
        /// Whether the user is active or not
        /// </summary>
        public bool? lockedOut { get; set; }
    }
}
