using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.Entities;
using Newtonsoft.Json;

namespace Laboremus_AuthorizationService.DTOs
{
    public class ApiResourcePropertyViewModel : Property
    {
        public int ApiResourceId { get; set; }

        [JsonIgnore]
        public ApiResource ApiResource { get; set; }
    }
}
