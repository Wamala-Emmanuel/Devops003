using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using IdentityModel;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Entities;
using Laboremus_AuthorizationService.Models;
using Laboremus_AuthorizationService.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Laboremus_AuthorizationService.Data
{
    public class SeedData
    {
        public static void Init(IServiceScope scope, IConfigurationRoot config)
        {
            var services = scope.ServiceProvider;
            var applicationDbContext = services.GetRequiredService<ApplicationDbContext>();
            var persistedGrantDbContext = services.GetRequiredService<PersistedGrantDbContext>();
            var configurationDbContext = services.GetRequiredService<ConfigurationDbContext>();

            applicationDbContext.Database.Migrate();
            persistedGrantDbContext.Database.Migrate();
            configurationDbContext.Database.Migrate();

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var roles = config.GetSection("SystemRoles").Get<List<string>>();

            #region Identity roles

            foreach (var role in roles)
            {
                var exists = roleManager.RoleExistsAsync(role).GetAwaiter().GetResult();
                if (!exists)
                {
                    roleManager.CreateAsync(new IdentityRole { Name = role }).GetAwaiter().GetResult();
                }
            }

            #endregion

            #region Identity Resources

            var identityResources = new List<IdentityResource>
            {
                new IdentityResource
                {
                    Description = "The openid connect claim",
                    DisplayName = "Your user identifier",
                    Emphasize = false,
                    Enabled = true,
                    Required = true,
                    Name = "openid",
                    ShowInDiscoveryDocument = true,
                    UserClaims = new List<IdentityClaim>()
                    {
                        new IdentityClaim {Type = JwtClaimTypes.Subject}
                    }
                },
                new IdentityResource
                {
                    Description = "The profile claim for returning the identity of a user",
                    DisplayName = "Your user profile",
                    Emphasize = true,
                    Enabled = true,
                    Required = false,
                    ShowInDiscoveryDocument = true,
                    Name = "profile",
                    UserClaims = new List<IdentityClaim>()
                    {
                        new IdentityClaim {Type = JwtClaimTypes.Name},
                        new IdentityClaim {Type = JwtClaimTypes.FamilyName},
                        new IdentityClaim {Type = JwtClaimTypes.GivenName},
                        new IdentityClaim {Type = JwtClaimTypes.MiddleName},
                        new IdentityClaim {Type = JwtClaimTypes.NickName},
                        new IdentityClaim {Type = JwtClaimTypes.PreferredUserName},
                        new IdentityClaim {Type = JwtClaimTypes.Profile},
                        new IdentityClaim {Type = JwtClaimTypes.Picture},
                        new IdentityClaim {Type = JwtClaimTypes.WebSite},
                        new IdentityClaim {Type = JwtClaimTypes.Gender},
                        new IdentityClaim {Type = JwtClaimTypes.BirthDate},
                        new IdentityClaim {Type = JwtClaimTypes.Locale},
                        new IdentityClaim {Type = JwtClaimTypes.ZoneInfo},
                        new IdentityClaim {Type = JwtClaimTypes.UpdatedAt},
                        //Required by eSign and Active sign
                        new IdentityClaim {Type = "tenant_id"}
                    }
                },
                new IdentityResource
                {
                    Description = "A claim for storing and returning the roles of a user",
                    DisplayName = "Your user roles",
                    Emphasize = false,
                    Enabled = true,
                    Required = false,
                    Name = "roles",
                    ShowInDiscoveryDocument = true,
                    UserClaims = new List<IdentityClaim>()
                    {
                        new IdentityClaim {Type = JwtClaimTypes.Role}
                    }
                },
                new IdentityResource
                {
                    Description = "A claim for a user's email",
                    DisplayName = "Your email address",
                    Emphasize = true,
                    Enabled = true,
                    Required = false,
                    Name = "email",
                    ShowInDiscoveryDocument = true,
                    UserClaims = new List<IdentityClaim>()
                    {
                        new IdentityClaim {Type = JwtClaimTypes.Email},
                        new IdentityClaim {Type = JwtClaimTypes.EmailVerified}
                    }
                }
            };

            var existingIdentityResources = configurationDbContext.IdentityResources;
            var identityResourcesToSave = identityResources
                .Where(resource => !existingIdentityResources.Any(x => x.Name == resource.Name))
                .ToList();

            if (identityResourcesToSave.Any())
            {
                configurationDbContext.IdentityResources.AddRange(identityResourcesToSave);
                configurationDbContext.SaveChanges();
            }

            #endregion

            #region System user

            var adminUser = config.GetSection("AdminUser").Get<AdminUser>();

            var user = userManager.FindByNameAsync(adminUser.Username).Result;

            if (user != null) return;
            {
                user = new ApplicationUser
                {
                    UserName = adminUser.Username
                };

                var result = userManager.CreateAsync(user, adminUser.Password).Result;
                if (!result.Succeeded) return;
                {
                    var claims = new List<Claim> { new Claim(JwtClaimTypes.Name, user.UserName) };
                    claims.AddRange(roles.Select(role => new Claim(JwtClaimTypes.Role, role)));
                    userManager.AddClaimsAsync(user, claims).GetAwaiter().GetResult();

                    foreach (var role in roles)
                    {
                        var upper = role.ToUpper();
                        var isInRole = userManager.IsInRoleAsync(user, role).Result;
                        if (isInRole) continue;
                        {
                            userManager.AddToRoleAsync(user, upper).GetAwaiter().GetResult();
                        }
                    }
                }
            }

            #endregion
        }
    }
}
