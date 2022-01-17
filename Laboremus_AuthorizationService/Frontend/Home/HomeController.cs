// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Laboremus_AuthorizationService.Frontend.Home
{
    [SecurityHeaders]
    public class HomeController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;

        public HomeController(IIdentityServerInteractionService interaction)
        {
            _interaction = interaction;
        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Shows the error page
        /// </summary>
        public async Task<IActionResult> Error(string errorId, string error = null)
        {
            var vm = new ErrorViewModel();

            if (!string.IsNullOrEmpty(error))
            {
                vm.Error = new ErrorMessage
                {
                    Error = error.Contains("invalid_client") ? "invalid_client" : error,
                    ErrorDescription = error
                };
            }
            else
            {
                var message = await _interaction.GetErrorContextAsync(errorId);
                if (message != null)
                {
                    vm.Error = message;
                }
            }

            return View("Error", vm);
        }
    }
}