using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Laboremus_AuthorizationService.DTOs
{
    public class CustomClaimViewModel
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public List<ClaimViewModel> Claims { get; set; }
    }
}
