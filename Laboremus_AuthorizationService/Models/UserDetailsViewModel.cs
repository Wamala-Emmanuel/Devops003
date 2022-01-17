using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace Laboremus_AuthorizationService.Models
{
    public class UserDetailsViewModel : NewUserViewModel
    {
        public string Id { get; set; }
        public bool LockedOut { get; set; }
    }
}
