using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using PluginBase;

namespace AuthServicePluginBase
{
    public interface IIdentityTokenPlugin : IPlugin
    {
        Task AdjustIdentityTokenClaims(List<Claim> claims);
    }
}
