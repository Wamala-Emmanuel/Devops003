using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Laboremus_AuthorizationService.Models
{
    public class UpdateUserViewModel : EditUserViewModel
    {
        public string Id { get; set; }
        public bool LockedOut { get; set; }
    }
}
