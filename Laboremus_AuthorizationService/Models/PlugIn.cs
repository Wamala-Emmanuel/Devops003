using System.Dynamic;

namespace Laboremus_AuthorizationService.Models
{
    public class PlugIn
    {
		public string Name { get; set; }
		public bool Enable { get; set; }
		public ExpandoObject ConfigurationData { get; set; }
    }
}
