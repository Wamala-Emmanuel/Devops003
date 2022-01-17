using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Laboremus_AuthorizationService.Models
{
    /// <summary>
    /// The stage at which the export request is at
    /// </summary>
    [DataContract]
    public enum ExportStatus
    {
        /// <summary>
        /// The request is still being processed
        /// </summary>
        [EnumMember]
        Processing = 1,

        /// <summary>
        /// The request was complete and successful
        /// </summary>
        [EnumMember]
        Complete = 2,
    }
}
