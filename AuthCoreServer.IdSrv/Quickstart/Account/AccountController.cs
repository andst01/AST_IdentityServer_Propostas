// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using AuthCoreServer.IdSrv.Entidades;
using AuthCoreServer.IdSrv.Models;
using AuthCoreServer.IdSrv.Quickstart.Account;
using IdentityModel;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServerHost.Quickstart.UI
{
    [SecurityHeaders]
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IEventService _events;

        public AccountController(
            UserManager<Usuario> userManager,
            SignInManager<Usuario> signInManager,
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IAuthenticationSchemeProvider schemeProvider,
            IEventService events)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _interaction = interaction;
            _clientStore = clientStore;
            _schemeProvider = schemeProvider;
            _events = events;
        }

        /// <summary>
        /// Entry point into the login workflow
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl)
        {
            // build a model so we know what to show on the login page
            var vm = await BuildLoginViewModelAsync(returnUrl);

            if (vm.IsExternalLoginOnly)
            {
                // we only have one option for logging in and it's an external provider
                return RedirectToAction("Challenge", "External", new { scheme = vm.ExternalLoginScheme, returnUrl });
            }

            return View(vm);
        }

        /// <summary>
        /// Handle postback from username/password login
        /// </summary>
        [HttpPost]
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
                    await _interaction.DenyAuthorizationAsync(context, AuthorizationError.AccessDenied);

                    // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                    if (context.IsNativeClient())
                    {
                        // The client is native, so this change in how to
                        // return the response is for better UX for the end user.
                        return this.LoadingPage("Redirect", model.ReturnUrl);
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
                var result = await _signInManager.PasswordSignInAsync(model.Username,
                                                                      model.Password,
                                                                      model.RememberLogin,
                                                                      lockoutOnFailure: true);
                if (result.RequiresTwoFactor)
                {
                    return RedirectToAction("LoginWith2fa", new
                    {
                        model.ReturnUrl,
                        model.RememberLogin
                    });
                }

                if (result.Succeeded)
                {
                    var user = await _userManager.FindByNameAsync(model.Username);
                    await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName,
                                                                        user.Id.ToString(),
                                                                        user.UserName,
                                                                        clientId: context?.Client.ClientId));

                    if (!user.TwoFactorEnabled)
                    {
                       
                        return RedirectToAction("EnableAuthenticator", new
                        {
                            returnUrl = model.ReturnUrl,
                            rememberMe = true
                        });
                    }

                    if (context != null)
                    {
                        if (context.IsNativeClient())
                        {
                            // The client is native, so this change in how to
                            // return the response is for better UX for the end user.
                            return this.LoadingPage("Redirect", model.ReturnUrl);
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

                await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "invalid credentials", clientId: context?.Client.ClientId));
                ModelState.AddModelError(string.Empty, AccountOptions.InvalidCredentialsErrorMessage);
            }

            // something went wrong, show form with error
            var vm = await BuildLoginViewModelAsync(model);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLogout(string returnUrl)
        {

            if (!string.IsNullOrEmpty(returnUrl))
            {
                await HttpContext.SignOutAsync();
                await _signInManager.SignOutAsync();
                await HttpContext.SignOutAsync("idsrv");
                await HttpContext.SignOutAsync("idsrv.external");
                await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Path = "/"
                };

                foreach (var cookie in Request.Cookies.Keys)
                {
                    Response.Cookies.Delete(cookie, cookieOptions);
                }
                // Response.Cookies.Delete(".AspNetCore.Identity.Application", cookieOptions);
                // Response.Cookies.Delete("idsrv.session", cookieOptions);


                return Redirect(returnUrl);
            }


            return Redirect("/");

        }
        /// <summary>
        /// Show logout page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId)
        {

            var vm = await BuildLogoutViewModelAsync(logoutId);

            // cria contexto de logout
            var logout = await _interaction.GetLogoutContextAsync(logoutId);

            await _signInManager.SignOutAsync();

            // Se você usa IdentityServer4 ou Duende, limpe o cookie principal também
            await HttpContext.SignOutAsync();

            // redireciona automaticamente
            if (logout?.PostLogoutRedirectUri != null)
            {
                return Redirect(logout.PostLogoutRedirectUri);
            }
            if (logout?.PostLogoutRedirectUri == null)
            {
                var client = await _clientStore.FindEnabledClientByIdAsync("react");

                if (client != null && client?.PostLogoutRedirectUris != null)
                {


                    // 1. Comando oficial do Identity
                    await _signInManager.SignOutAsync();
                    await HttpContext.SignOutAsync("idsrv");
                    await HttpContext.SignOutAsync("idsrv.external");
                    await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

                    // 2. REMOÇÃO MANUAL DOS COOKIES (O pulo do gato)
                    // Precisamos garantir que o domínio e o path batam exatamente
                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.None, // Importante para cross-origin (5001 -> 5173)
                        Path = "/"
                    };

                    Response.Cookies.Delete(".AspNetCore.Identity.Application", cookieOptions);
                    Response.Cookies.Delete("idsrv.session", cookieOptions);

                    return Redirect(client.PostLogoutRedirectUris.First());
                }
            }

            return Redirect("~/");
        }

        /// <summary>
        /// Handle logout page postback
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(LogoutInputModel model)
        {
            // build a model so the logged out page knows what to display
            var vm = await BuildLoggedOutViewModelAsync(model.LogoutId);

            var logout = await _interaction.GetLogoutContextAsync(model.LogoutId);

            if (User?.Identity.IsAuthenticated == true)
            {
                // delete local authentication cookie
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
                string url = Url.Action("Logout", new { logoutId = vm.LogoutId });

                // this triggers a redirect to the external provider for sign-out
                return SignOut(new AuthenticationProperties { RedirectUri = url }, vm.ExternalAuthenticationScheme);
            }

            return View("LoggedOut", vm);
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }


        /*****************************************/
        /* helper APIs for the AccountController */
        /*****************************************/
        private async Task<LoginViewModel> BuildLoginViewModelAsync(string returnUrl)
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (context?.IdP != null && await _schemeProvider.GetSchemeAsync(context.IdP) != null)
            {
                var local = context.IdP == IdentityServer4.IdentityServerConstants.LocalIdentityProvider;

                // this is meant to short circuit the UI and only trigger the one external IdP
                var vm = new LoginViewModel
                {
                    EnableLocalLogin = local,
                    ReturnUrl = returnUrl,
                    Username = context?.LoginHint,
                };

                if (!local)
                {
                    vm.ExternalProviders = new[] { new ExternalProvider { AuthenticationScheme = context.IdP } };
                }

                return vm;
            }

            var schemes = await _schemeProvider.GetAllSchemesAsync();

            var providers = schemes
                .Where(x => x.DisplayName != null)
                .Select(x => new ExternalProvider
                {
                    DisplayName = x.DisplayName ?? x.Name,
                    AuthenticationScheme = x.Name
                }).ToList();

            var allowLocal = true;
            if (context?.Client.ClientId != null)
            {
                var client = await _clientStore.FindEnabledClientByIdAsync(context.Client.ClientId);
                if (client != null)
                {
                    allowLocal = client.EnableLocalLogin;

                    if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
                    {
                        providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
                    }
                }
            }

            return new LoginViewModel
            {
                AllowRememberLogin = AccountOptions.AllowRememberLogin,
                EnableLocalLogin = allowLocal && AccountOptions.AllowLocalLogin,
                ReturnUrl = returnUrl,
                Username = context?.LoginHint,
                ExternalProviders = providers.ToArray()
            };
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
            var vm = new LogoutViewModel { LogoutId = logoutId, ShowLogoutPrompt = AccountOptions.ShowLogoutPrompt };

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
                AutomaticRedirectAfterSignOut = AccountOptions.AutomaticRedirectAfterSignOut,
                PostLogoutRedirectUri = logout?.PostLogoutRedirectUri,
                ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout?.ClientName,
                SignOutIframeUrl = logout?.SignOutIFrameUrl,
                LogoutId = logoutId
            };

            if (User?.Identity.IsAuthenticated == true)
            {
                var idp = User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
                if (idp != null && idp != IdentityServer4.IdentityServerConstants.LocalIdentityProvider)
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

        #region MFA



        [HttpGet]
        public async Task<IActionResult> EnableAuthenticator(string returnUrl = null, bool rememberMe = false)
        {

            var user = await _userManager.GetUserAsync(User);

            var key = await _userManager.GetAuthenticatorKeyAsync(user);

            if (string.IsNullOrEmpty(key))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                key = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            var authUri = GenerateQrCodeUri(user.Email, key);

            var model = new EnableAuthenticatorViewModel
            {
                SharedKey = key,
                AuthenticatorUri = authUri,
                QrCodeImageSource = GenerateQrCodeImage(authUri),
                ReturnUrl = returnUrl,
                
            };

            return View(model);

            //var model = new EnableAuthenticatorViewModel
            //{
            //    SharedKey = key,
            //    AuthenticatorUri = GenerateQrCodeUri(user.Email, key)
            //};

            //return View(model);
        }

        private string GenerateQrCodeUri(string email, string unformattedKey)
        {

            return string.Format(
                "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6",
                "SeuApp",
                email,
                unformattedKey);
        }

        private string GenerateQrCodeImage(string authenticatorUri)
        {
            #region antigo
            //using (var qrGenerator = new QRCodeGenerator())
            //{
            //    // Cria os dados do QR Code com nível de correção 'Q' (25% de perda aceitável)
            //    using (var qrCodeData = qrGenerator.CreateQrCode(authenticatorUri, QRCodeGenerator.ECCLevel.Q))
            //    {
            //        // Gera a imagem em formato de bytes PNG
            //        using (var qrCode = new PngByteQRCode(qrCodeData))
            //        {
            //            byte[] qrCodeBytes = qrCode.GetGraphic(20);

            //            // Converte para Base64 para ser exibido diretamente na tag <img> do HTML
            //            return string.Format("data:image/png;base64,{0}", Convert.ToBase64String(qrCodeBytes));
            //        }


            //    }
            //}

            #endregion

            string QrCodeImageSource = "";
            using (var qrGenerator = new QRCodeGenerator())
            {
                // Nível Q é ótimo para garantir leitura mesmo com sujeira/tamanho pequeno
                var qrCodeData = qrGenerator.CreateQrCode(authenticatorUri, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new PngByteQRCode(qrCodeData);

                // Mude de 20 para 4. Isso gera uma imagem de aprox. 200x200px dependendo da URL
                byte[] qrCodeBytes = qrCode.GetGraphic(4);

                // Retorne a string pura (o prefixo colocaremos na View para ficar mais limpo)
                return  string.Format("data:image/png;base64,{0}", Convert.ToBase64String(qrCodeBytes));
            }


        }

        public async Task<IActionResult> EnableAuthenticator(EnableAuthenticatorViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            var verificationCode = model.Code.Replace(" ", "").Replace("-", "");

            var isValid = await _userManager.VerifyTwoFactorTokenAsync(
                user,
                _userManager.Options.Tokens.AuthenticatorTokenProvider,
                verificationCode);

            if (!isValid)
            {
                ModelState.AddModelError("Code", "Código inválido");
                return View(model);
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);

            await _signInManager.SignOutAsync();

            // Redireciona para o login passando o ReturnUrl original
            return RedirectToAction("Login", new { ReturnUrl = model.ReturnUrl });

           // return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> LoginWith2fa(bool rememberMe, string returnUrl)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
                throw new Exception("Usuário inválido");

            return View(new LoginWith2faViewModel
            {
                ReturnUrl = returnUrl,
                RememberMe = rememberMe
            });
        }

        [HttpPost]
        public async Task<IActionResult> LoginWith2fa(LoginWith2faViewModel model)
        {
            /* var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

             var code = model.TwoFactorCode.Replace(" ", "").Replace("-", "");

             var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(
                 code,
                 model.RememberMe,
                 rememberClient: false);

             if (result.Succeeded)
             {
                 return Redirect(model.ReturnUrl);
             }

             ModelState.AddModelError("", "Código inválido");
             return View(model); */

            // 1. Busca o usuário que está no meio do processo de 2FA (via cookie temporário)
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login"); // Ou lance uma exceção apropriada
            }

            var code = model.TwoFactorCode.Replace(" ", "").Replace("-", "");

            // 2. IMPORTANTE: Use model.RememberMachine para que o checkbox da View funcione
            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(
                code,
                model.RememberMe,
                model.RememberMachine);

            if (result.Succeeded)
            {
                return Redirect(model.ReturnUrl);
            }

            // 3. Verificação de conta bloqueada (opcional, mas boa prática)
            if (result.IsLockedOut)
            {
                return RedirectToAction("Lockout");
            }

            ModelState.AddModelError(string.Empty, "Código inválido.");
            return View(model);
        }

        #endregion
    }
}