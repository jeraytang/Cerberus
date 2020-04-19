using System;
using System.Linq;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.SecurityTokenService.Extensions;
using IdentityServer4.Models;
using IdentityServer4.SecurityTokenService.Models;
using IdentityServer4.SecurityTokenService.Models.Account;
using IdentityServer4.SecurityTokenService.Models.Settings;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.SecurityTokenService.Controllers
{
    public partial class AccountController
    {
        [HttpGet("SentEmailConfirmation")]
        public IActionResult SentEmailConfirmation()
        {
            return View();
        }

        [HttpGet("NotAllowed")]
        public IActionResult NotAllowed()
        {
            return View();
        }

        //
        // GET: /Account/Login
        [HttpGet("Login")]
        public async Task<IActionResult> Login(string returnUrl)
        {
            // build a model so we know what to show on the login page
            var vm = await BuildLoginViewModelAsync(returnUrl);
            return View(vm);
        }

        //
        // POST: /Account/Login
        [HttpPost("Login")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginInputModel model, string button)
        {
            // check if we are in the context of an authorization request
            var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);

            // the user clicked the "cancel" button
            if (button != "login")
            {
                if (context != null)
                {
                    // if the user cancels, send a result back into IdentityServer as if they
                    // denied the consent (even if this client does not require consent).
                    // this will send back an access denied OIDC error response to the client.
                    await _interaction.GrantConsentAsync(context, ConsentResponse.Denied);

                    // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                    if (await _clientStore.IsPkceClientAsync(context.ClientId))
                    {
                        // if the client is PKCE then we assume it's native, so this change in how to
                        // return the response is for better UX for the end user.
                        return View("Redirect", new RedirectViewModel {RedirectUrl = model.ReturnUrl});
                    }

                    return Redirect(model.ReturnUrl);
                }
                else
                {
                    // since we don't have a valid context, then we just go back to the home page
                    return Redirect("~/");
                }
            }

            if (ModelState.IsValid)
            {
                // validate username/password against in-memory store
                var user = await _userManager.FindByNameAsync(model.Username);
                if (user == null)
                {
                    user = await _userManager.FindByEmailAsync(model.Username);
                }

                if (user == null)
                {
                    ModelState.AddModelError(nameof(model.Username), "用户名错误！");
                    return View(await BuildLoginViewModelAsync(model));
                }

                var result = await _signInManager.PasswordSignInAsync(user, model.Password,
                    model.RememberLogin, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    await _events.RaiseAsync(
                        new UserLoginSuccessEvent(user.UserName, user.Id, user.Email,
                            clientId: context?.ClientId));

                    if (context != null)
                    {
                        if (await _clientStore.IsPkceClientAsync(context.ClientId))
                        {
                            // if the client is PKCE then we assume it's native, so this change in how to
                            // return the response is for better UX for the end user.
                            return View("Redirect", new RedirectViewModel {RedirectUrl = model.ReturnUrl});
                        }

                        // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                        return Redirect(model.ReturnUrl);
                    }

                    // request for a local page
                    if (Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    else if (string.IsNullOrEmpty(model.ReturnUrl))
                    {
                        return Redirect("~/");
                    }
                    else
                    {
                        // user might have clicked on a malicious link - should be logged
                        throw new Exception("invalid return URL");
                    }
                }

                if (result.RequiresTwoFactor)
                {
                    return RedirectToAction(nameof(SendCode),
                        new {model.ReturnUrl, RememberMe = model.RememberLogin});
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning(2, "User account locked out.");
                    return View("Locked");
                }
                else
                {
                    await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "invalid credentials",
                        clientId: context?.ClientId));
                    ModelState.AddModelError(nameof(model.Password), "密码git错误！");
                }
            }

            // something went wrong, show form with error
            var vm = await BuildLoginViewModelAsync(model);
            return View(vm);
        }


        [HttpGet("Logout")]
        public async Task<IActionResult> Logout(string logoutId)
        {
            // build a model so the logout page knows what to display
            var vm = await BuildLogoutViewModelAsync(logoutId);

            if (vm.ShowLogoutPrompt == false)
            {
                // if the request for logout was properly authenticated from IdentityServer, then
                // we don't need to show the prompt and can just log the user out directly.
                return await Logout(vm);
            }

            return View(vm);
        }

        [HttpPost("Logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(LogoutInputModel model)
        {
            // build a model so the logged out page knows what to display
            var vm = await BuildLoggedOutViewModelAsync(model.LogoutId);

            if (User?.Identity.IsAuthenticated == true)
            {
                await _signInManager.SignOutAsync();
                // raise the logout event
                await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
            }

            // check if we need to trigger sign-out at an upstream identity provider
            if (vm.TriggerExternalSignout)
            {
                // build a return URL so the upstream provider will redirect back
                // to us after the user has logged out. this allows us to then
                // complete our single sign-out processing.
                var url = Url.Action("Logout", new {logoutId = vm.LogoutId});

                // this triggers a redirect to the external provider for sign-out
                return SignOut(new AuthenticationProperties {RedirectUri = url}, vm.ExternalAuthenticationScheme);
            }

            return View("LoggedOut", vm);
        }

        [HttpPost("UserName")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUserNameAsync(ChangeUserNameViewModel model)
        {
            if (!ModelState.IsValid)
            {
                SendNotificationFromModelErrors("Account", "Settings");
                return RedirectToAction("Account", "Settings");
            }
            else
            {
                var user = await GetCurrentUserAsync();
                if (user == null)
                {
                    // 当前未登录不应该有权限进入
                    return NotFound();
                }

                if (user.UserName == "admin")
                {
                    SendNotification("Account", "Settings", NotificationType.Warning,
                        "Should not change admin username");
                    return RedirectToAction("Account", "Settings");
                }


                if (model.NewUserName.Trim() == user.UserName)
                {
                    SendNotification("Account", "Settings", NotificationType.Warning, "Username didn't changed");
                }
                else
                {
                    var hopeUser = await _userManager.FindByNameAsync(model.NewUserName);
                    if (hopeUser == null)
                    {
                        user.UserName = model.NewUserName;
                        await _userManager.UpdateAsync(user);
                        await _signInManager.SignInAsync(user, true);
                        SendNotification("Account", "Settings", NotificationType.Success, "Username changed");
                    }
                    else
                    {
                        SendNotification("Account", "Settings", NotificationType.Error, "Username already exists");
                    }
                }
            }

            return RedirectToAction("Account", "Settings");
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost("Password")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePasswordAsync(ChangepasswordInputModel model)
        {
            if (!ModelState.IsValid)
            {
                SendNotificationFromModelErrors("Security", "Settings");
                return RedirectToAction("Security", "Settings");
            }
            else
            {
                var user = await GetCurrentUserAsync();
                if (user == null)
                {
                    return RedirectToAction("Security", "Settings");
                }

                // No password then set it
                if (string.IsNullOrWhiteSpace(user.PasswordHash))
                {
                    var result = await _userManager.AddPasswordAsync(user, model.NewPassword);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        SendNotification("Security", "Settings",
                            new NotificationViewModel(NotificationType.Success, "Set password success"));
                    }
                    else
                    {
                        SendNotification("Security", "Settings",
                            new NotificationViewModel(NotificationType.Error,
                                string.Join(",", result.Errors.Select(x => x.Description))));
                    }
                }
                else
                {
                    var result = await _userManager.ChangePasswordAsync(user,
                        model.OldPassword,
                        model.NewPassword);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        _logger.LogInformation(3, "User changed their password successfully.");
                        SendNotification("Security", "Settings",
                            new NotificationViewModel(NotificationType.Success, "Change password success"));
                        return RedirectToAction("Security", "Settings");
                    }
                    else
                    {
                        SendNotification("Security", "Settings",
                            new NotificationViewModel(NotificationType.Error,
                                string.Join(",", result.Errors.Select(x => x.Description))));
                    }
                }
            }

            return RedirectToAction("Security", "Settings");
        }

        [HttpPost("EmailConfirmation")]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> EmailConfirmation()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return NotFound();
                }
                else
                {
                    user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                    if (user == null)
                    {
                        return NotFound();
                    }
                }
            }

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Action("ConfirmEmail", "Account", new {userId = user.Id, code},
                protocol: HttpContext.Request.Scheme);
            await _emailSender.SendEmailAsync(user.Email, "Confirm your account",
                "Please confirm your account by clicking this link: <a href=\"" + callbackUrl + "\">link</a>");

            return View("SentEmailConfirmation");
        }

        #region Helpers

        private async Task<LoginViewModel> BuildLoginViewModelAsync(string returnUrl)
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (context?.IdP != null && await _schemeProvider.GetSchemeAsync(context.IdP) != null)
            {
                var local = context.IdP == IdentityServerConstants.LocalIdentityProvider;

                // this is meant to short circuit the UI and only trigger the one external IdP
                var vm = new LoginViewModel
                {
                    EnableLocalLogin = local, ReturnUrl = returnUrl, Username = context.LoginHint,
                };

                if (!local)
                {
                    vm.ExternalProviders = new[] {new ExternalProvider {Name = context.IdP}};
                }

                return vm;
            }

            var schemes = await _schemeProvider.GetAllSchemesAsync();

            var providers = schemes
                .Where(x => x.DisplayName != null ||
                            (x.Name.Equals(_globalOptions.WindowsAuthenticationSchemeName,
                                StringComparison.OrdinalIgnoreCase))
                )
                .Select(x => new ExternalProvider {DisplayName = x.DisplayName, Name = x.Name})
                .ToList();

            var allowLocal = true;
            if (context?.ClientId != null)
            {
                var client = await _clientStore.FindEnabledClientByIdAsync(context.ClientId);
                if (client != null)
                {
                    allowLocal = client.EnableLocalLogin;

                    if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
                    {
                        providers = providers.Where(provider =>
                            client.IdentityProviderRestrictions.Contains(provider.Name)).ToList();
                    }
                }
            }

            var viewModel = new LoginViewModel
            {
                AllowRememberLogin = _globalOptions.AllowRememberLogin,
                EnableLocalLogin = allowLocal && _globalOptions.AllowLocalLogin,
                ReturnUrl = returnUrl,
                Username = context?.LoginHint,
                ExternalProviders = providers.ToArray()
            };
            viewModel.VisibleExternalProviders =
                viewModel.ExternalProviders.Where(x => !string.IsNullOrWhiteSpace(x.DisplayName));
            return viewModel;
        }

        private async Task<LoginViewModel> BuildLoginViewModelAsync(LoginInputModel model)
        {
            var vm = await BuildLoginViewModelAsync(model.ReturnUrl);
            vm.Username = model.Username;
            vm.RememberLogin = model.RememberLogin;
            return vm;
        }

        private async Task<LogoutViewModel> BuildLogoutViewModelAsync(string logoutId)
        {
            var vm = new LogoutViewModel {LogoutId = logoutId, ShowLogoutPrompt = _globalOptions.ShowLogoutPrompt};

            if (User?.Identity.IsAuthenticated != true)
            {
                // if the user is not authenticated, then just show logged out page
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            var context = await _interaction.GetLogoutContextAsync(logoutId);
            if (context?.ShowSignoutPrompt == false)
            {
                // it's safe to automatically sign-out
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            // show the logout prompt. this prevents attacks where the user
            // is automatically signed out by another malicious web page.
            return vm;
        }

        private async Task<LoggedOutViewModel> BuildLoggedOutViewModelAsync(string logoutId)
        {
            // get context information (client name, post logout redirect URI and iframe for federated signout)
            var logout = await _interaction.GetLogoutContextAsync(logoutId);

            var vm = new LoggedOutViewModel
            {
                AutomaticRedirectAfterSignOut = _globalOptions.AutomaticRedirectAfterSignOut,
                PostLogoutRedirectUri = logout?.PostLogoutRedirectUri,
                ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout.ClientName,
                SignOutIframeUrl = logout?.SignOutIFrameUrl,
                LogoutId = logoutId
            };

            if (User?.Identity.IsAuthenticated == true)
            {
                var idp = User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
                if (idp != null && idp != IdentityServerConstants.LocalIdentityProvider)
                {
                    var providerSupportsSignout = await HttpContext.GetSchemeSupportsSignOutAsync(idp);
                    if (providerSupportsSignout)
                    {
                        if (vm.LogoutId == null)
                        {
                            // if there's no current logout context, we need to create one
                            // this captures necessary info from the current logged in user
                            // before we signout and redirect away to the external IdP for signout
                            vm.LogoutId = await _interaction.CreateLogoutContextAsync();
                        }

                        vm.ExternalAuthenticationScheme = idp;
                    }
                }
            }

            return vm;
        }

        #endregion
    }
}