using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel.Client;
using Laboremus_AuthorizationService.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Laboremus_AuthorizationService.Core.Helpers
{
    public static class OpenIdConnectMiddleware
    {
        private static string GenerateState()
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());

        }


        public static IServiceCollection AddExternalOidcProviders(this IServiceCollection services, ILogger logger, IConfiguration configuration)
        {
            var authentication = services.AddAuthentication(options =>
            {
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
                options.DefaultSignOutScheme = IdentityConstants.ExternalScheme;
            });

            var providers = configuration.GetSection("ExternalProviders").Get<List<ExternalProvider>>();

            if (providers == null)
            {
                return services;
            }

            foreach (var provider in providers)
            {
                var authority = $"{provider.Settings.Instance}";
                if (!string.IsNullOrEmpty(provider.Settings.Domain))
                {
                    authority += $"/{provider.Settings.Domain}/{provider.Settings.SignUpSignInPolicyId}/v2.0";
                }
                else
                {
                    authority += $"/{provider.Settings.TenantId}";
                }

                authentication.AddOpenIdConnect(provider.Id, provider.DisplayName, options =>
                {
                    options.SaveTokens = true;

                    options.Authority = authority;
                    options.ClientId = $"{provider.Settings.ClientId}";
                    options.CallbackPath = new PathString(provider.Settings.CallbackPath);
                    options.SignedOutCallbackPath = new PathString(provider.Settings.PostLogoutCallbackPath);

                    if (!string.IsNullOrEmpty(provider.Settings.ResponseType))
                    {
                        options.ResponseType = provider.Settings.ResponseType;
                    }

                    if (!string.IsNullOrEmpty(provider.Settings.Scopes))
                    {
                        options.Scope.Add(provider.Settings.Scopes);
                    }

                    options.GetClaimsFromUserInfoEndpoint = true;

                    options.ClaimActions.MapUniqueJsonKey("sub", "sub");
                    options.ClaimActions.MapUniqueJsonKey("name", "name");
                    options.ClaimActions.MapUniqueJsonKey("given_name", "given_name");
                    options.ClaimActions.MapUniqueJsonKey("family_name", "family_name");
                    options.ClaimActions.MapUniqueJsonKey("email", "email");
                    options.ClaimActions.MapUniqueJsonKey("role", "role");
                    options.ClaimActions.MapUniqueJsonKey("phone_number", ClaimTypes.MobilePhone);

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true
                    };

                    options.Events.OnRedirectToIdentityProvider = context =>
                    {
                        // a0b50413
                        context.ProtocolMessage.State = GenerateState();
                        return Task.FromResult(0);
                    };

                    options.Events.OnTokenValidated = async context =>
                    {
                        var identity = new ClaimsIdentity(context.HttpContext.User.Identity);
                        context.Principal.AddIdentity(identity);

                        await Task.Yield();
                    };

                    options.Events.OnAuthorizationCodeReceived = async context =>
                    {
                        using (var httpClient = new HttpClient())
                        {
                            var discovery = await httpClient.GetDiscoveryDocumentAsync(context.Options.Authority);
                            if (!discovery.IsError)
                            {
                                var tokenResponse = await httpClient.RequestAuthorizationCodeTokenAsync(
                                    new AuthorizationCodeTokenRequest
                                    {
                                        ClientId = context.TokenEndpointRequest.ClientId,
                                        ClientSecret = provider.Settings.ClientSecret,
                                        Address = discovery.TokenEndpoint,
                                        Code = context.ProtocolMessage.Code,
                                        RedirectUri = context.TokenEndpointRequest.RedirectUri,
                                        
                                    });

                                if (tokenResponse.IsError)
                                {
                                    logger.LogError(tokenResponse.ErrorDescription);
                                }
                                else
                                {
                                    var userInfoResponse = await httpClient.GetUserInfoAsync(new UserInfoRequest
                                    {
                                        Address = discovery.UserInfoEndpoint,
                                        Token = tokenResponse.AccessToken
                                    });

                                    if (userInfoResponse.IsError)
                                        logger.LogError(userInfoResponse.Error);

                                    var identity = new ClaimsIdentity(IdentityConstants.ExternalScheme);
                                    identity.AddClaims(userInfoResponse.Claims);

                                    //identity.AddClaim(new Claim("access_token", tokenResponse.AccessToken));
                                    //identity.AddClaim(new Claim("expires_at",
                                    //    DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn)
                                    //        .ToString(CultureInfo.CurrentCulture)));
                                    //identity.AddClaim(new Claim("refresh_token", tokenResponse.RefreshToken));
                                    //identity.AddClaim(new Claim("token_type", tokenResponse.TokenType));
                                    //identity.AddClaim(new Claim("id_token", tokenResponse.IdentityToken));

                                    context.HttpContext.User = new ClaimsPrincipal(identity);

                                    context.HandleCodeRedemption(tokenResponse.AccessToken, tokenResponse.IdentityToken);
                                }

                                await Task.Yield();
                            }
                            else
                            {
                                logger.LogError(discovery.Error, discovery.Exception);
                            }
                        }
                    };

                    options.Events.OnRedirectToIdentityProviderForSignOut = context =>
                    {
                        var idTokenHint = context.HttpContext.User.FindFirst("id_token");
                        if (idTokenHint != null)
                        {
                            context.ProtocolMessage.IdTokenHint = idTokenHint.Value;
                        }

                        return Task.FromResult(0);
                    };

                    options.Events.OnRemoteFailure = context =>
                    {
                        context.HandleResponse();

                        // Handle the error code that Azure AD B2C throws when trying to reset a password from the login page 
                        // because password reset is not supported by a "sign-up or sign-in policy"
                        // AADB2C90289

                        if (context.Failure is OpenIdConnectProtocolException)
                        {
                            if (context.Failure.Message.Contains("AADB2C90289"))
                            {
                                context.Response.Redirect("/Home/Error?error=" + WebUtility.UrlEncode(context.Failure.Message));
                            }
                            else if (context.Failure.Message.Contains("access_denied"))
                            {
                                context.Response.Redirect("/");
                            }
                            else
                            {
                                context.Response.Redirect($"/Home/Error?error={WebUtility.UrlEncode(context.Failure.Message)}");
                            }
                        }
                        else
                        {
                            context.Response.Redirect($"/Home/Error?error={WebUtility.UrlEncode(context.Failure.Message)}");
                        }

                        return Task.CompletedTask;
                    };
                });
            }

            return services;
        }
    }
}
