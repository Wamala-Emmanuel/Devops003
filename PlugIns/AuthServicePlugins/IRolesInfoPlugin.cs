using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using PluginBase;

namespace AuthServicePluginBase
{
    public interface IRolesInfoPlugin : IPlugin
	{
		/// <summary>
		/// Gets the name.
		/// </summary>
		/// <value>
		/// The name.
		/// </value>
		new string Name { get; }
		
		/// <summary>
		/// Gets the role information object.
		/// </summary>
		/// <param name="ssn">The SSN.</param>
		/// <returns></returns>
		Task<string> GetRoleInfoObject(string ssn);

        /// <summary>
        /// Gets the role information object.
        /// </summary>
        /// <param name="additionalLocalClaims">The additional local claims.</param>
        /// <returns></returns>
        Task<string> GetRoleInfoObject(List<Claim> additionalLocalClaims);
    }
}
