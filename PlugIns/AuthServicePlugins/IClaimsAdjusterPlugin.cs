using System.Collections.Generic;
using System.Security.Claims;
using PluginBase;

namespace AuthServicePluginBase
{
	public interface IClaimsAdjusterPlugin : IPlugin
	{
		new string Name { get; }
		void AdjustAccessTokenClaims(List<Claim> localClaims, string crmResponse);
	}
}
