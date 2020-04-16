using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.SecurityTokenService.Common;
using IdentityServer4.SecurityTokenService.Data;
using IdentityServer4.SecurityTokenService.Extensions;
using IdentityServer4.SecurityTokenService.Models;
using IdentityServer4.SecurityTokenService.Models.Settings;
using IdentityServer4.SecurityTokenService.Services;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Newtonsoft.Json;

namespace IdentityServer4.SecurityTokenService.Controllers
{
	[Authorize]
	[Route("settings")]
	public class SettingsController : BaseController
	{
		private readonly UserManager<IdentityUser> _userManager;
		private readonly SignInManager<IdentityUser> _signInManager;
		private readonly ISmsSender _smsSender;
		private readonly ILogger _logger;
		private readonly IIdentityServerInteractionService _interaction;
		private readonly IClientStore _clientStore;
		private readonly IResourceStore _resources;
		private readonly IEventService _events;
		private readonly UrlEncoder _urlEncoder;
		private readonly STSOptions _options;
		private readonly IdentityServer4.SecurityTokenService.Data.IdentityDbContext _dbContext;

		public SettingsController(IdentityServer4.SecurityTokenService.Data.IdentityDbContext dbContext,
			UserManager<IdentityUser> userManager,
			SignInManager<IdentityUser> signInManager,
			ISmsSender smsSender,
			IClientStore clientStore,
			IResourceStore resources,
			IEventService events,
			UrlEncoder urlEncoder,
			IIdentityServerInteractionService interaction,
			STSOptions options,
			ILoggerFactory loggerFactory, IRedirectNotificationService redirectNotificationService) : base(
			redirectNotificationService)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_smsSender = smsSender;
			_logger = loggerFactory.CreateLogger<SettingsController>();
			_interaction = interaction;
			_clientStore = clientStore;
			_resources = resources;
			_events = events;
			_urlEncoder = urlEncoder;
			_options = options;
			_dbContext = dbContext;
		}

		[HttpGet]
		public IActionResult Index()
		{
			return RedirectToAction(nameof(Profile));
		}

		[HttpGet("Profile")]
		public async Task<IActionResult> Profile()
		{
			var user = await GetCurrentUserAsync();
			var claims = await _userManager.GetClaimsAsync(user);
			var picture = claims.FirstOrDefault(x => x.Type == "picture")?.Value;
			var model = new ProfileViewModel
			{
				Birthday = claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Birthdate)?.Value,
				GivenName = claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.GivenName)?.Value,
				FamilyName = claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.FamilyName)?.Value,
				Website = claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Website)?.Value,
				Location = claims.FirstOrDefault(x => x.Type == "location")?.Value,
				Bio = claims.FirstOrDefault(x => x.Type == "bio")?.Value,
				Company = claims.FirstOrDefault(x => x.Type == "company")?.Value,
				NickName = claims.FirstOrDefault(x => x.Type == "nickname")?.Value,
				Telephone = claims.FirstOrDefault(x => x.Type == "telephone")?.Value,
				Picture = string.IsNullOrWhiteSpace(picture) ? "image/head.jpeg" : picture
			};
			var sex = claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Gender)?.Value;
			if (!string.IsNullOrWhiteSpace(sex))
			{
				model.Sex = Enum.Parse<Sex>(sex);
			}

			return View(model);
		}

		[HttpPost("Profile")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Profile(ProfileViewModel model)
		{
			if (ModelState.IsValid)
			{
				var userId = _userManager.GetUserId(HttpContext.User);
				var user = await _userManager.FindByIdAsync(userId);
				if (user == null)
				{
					return NotFound();
				}

				DateTime.TryParse(model.Birthday, out var birthday);
				await _dbContext.AddOrUpdateClaimsAsync(user.Id,
					new Dictionary<string, string>
					{
						{"bio", model.Bio},
						{"company", model.Company},
						{"location", model.Location},
						{"telephone", model.Telephone},
						{"nickname", model.NickName},
						{
							JwtRegisteredClaimNames.Birthdate,
							birthday == DateTime.MinValue ? null : birthday.ToString("yyyy-MM-dd")
						},
						{JwtRegisteredClaimNames.Gender, model.Sex.ToString()},
						{JwtRegisteredClaimNames.Website, model.Website},
						{JwtRegisteredClaimNames.GivenName, model.GivenName},
						{JwtRegisteredClaimNames.FamilyName, model.FamilyName}
					});
			}

			return View(model);
		}

		// POST: /Manage/RemoveLogin
		[HttpPost("RemoveLogin")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> RemoveLogin(RemoveLoginViewModel account)
		{
			var user = await GetCurrentUserAsync();
			var message = "Remove login failed";
			if (user != null)
			{
				var result = await _userManager.RemoveLoginAsync(user, account.LoginProvider, account.ProviderKey);
				if (result.Succeeded)
				{
					await _signInManager.SignInAsync(user, isPersistent: false);
					message = "Remove login success";
				}
			}

			SendNotification(nameof(ExternalLogins), "Settings", NotificationType.Success, message);
			return RedirectToAction("ExternalLogins", "Settings");
		}

		//
		// GET: /Manage/AddPhoneNumber
		[HttpGet("AddPhoneNumber")]
		public IActionResult AddPhoneNumber()
		{
			return View();
		}

		//
		// POST: /Manage/AddPhoneNumber
		[HttpPost("AddPhoneNumber")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			// Generate the token and send it
			var user = await GetCurrentUserAsync();
			var code = await _userManager.GenerateChangePhoneNumberTokenAsync(user, model.PhoneNumber);
			await _smsSender.SendSmsAsync(model.PhoneNumber, "Your security code is: " + code);
			return RedirectToAction(nameof(VerifyPhoneNumber), new {model.PhoneNumber});
		}

		//
		// POST: /Manage/ResetAuthenticatorKey
		[HttpPost("ResetAuthenticatorKey")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ResetAuthenticatorKey()
		{
			var user = await GetCurrentUserAsync();
			if (user != null)
			{
				await _userManager.ResetAuthenticatorKeyAsync(user);
				_logger.LogInformation(1, "User reset authenticator key.");
			}

			return RedirectToAction(nameof(Profile), "Settings");
		}

		//
		// POST: /Manage/GenerateRecoveryCode
		[HttpPost("GenerateRecoveryCode")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> GenerateRecoveryCode()
		{
			var user = await GetCurrentUserAsync();
			if (user != null)
			{
				var codes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 5);
				_logger.LogInformation(1, "User generated new recovery code.");
				return View("DisplayRecoveryCodes", new DisplayRecoveryCodesViewModel {Codes = codes});
			}

			return View("Error");
		}

		//
		// POST: /Manage/EnableTwoFactorAuthentication
		[HttpPost("EnableTwoFactorAuthentication")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EnableTwoFactorAuthentication()
		{
			var user = await GetCurrentUserAsync();
			if (user != null)
			{
				await _userManager.SetTwoFactorEnabledAsync(user, true);
				await _signInManager.SignInAsync(user, isPersistent: false);
				_logger.LogInformation(1, "User enabled two-factor authentication.");
			}

			return RedirectToAction(nameof(Security));
		}

		[HttpGet("DisableTwoFactorAuthentication")]
		public IActionResult DisableTwoFactorAuthentication()
		{
			return View();
		}

		//
		// POST: /Manage/DisableTwoFactorAuthentication
		[HttpPost("DisableTwoFactorAuthentication")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DisableTwoFactorAuthentication(DisableTwoFactorAuthenticationViewModel model)
		{
			var user = await GetCurrentUserAsync();
			if (user != null)
			{
				await _userManager.SetTwoFactorEnabledAsync(user, false);
				await _signInManager.SignInAsync(user, isPersistent: false);
				_logger.LogInformation(2, "User disabled two-factor authentication.");
				SendNotification(nameof(Security), "Settings", NotificationType.Success,
					"Disabled two-factor authentication success.");
			}

			return RedirectToAction(nameof(Security), "Settings");
		}

		//
		// GET: /Manage/VerifyPhoneNumber
		[HttpGet("VerifyPhoneNumber")]
		public async Task<IActionResult> VerifyPhoneNumber(string phoneNumber)
		{
			var code = await _userManager.GenerateChangePhoneNumberTokenAsync(await GetCurrentUserAsync(), phoneNumber);
			await _smsSender.SendSmsAsync(phoneNumber, code);
			// Send an SMS to verify the phone number
			return phoneNumber == null
				? View("Error")
				: View(new VerifyPhoneNumberViewModel {PhoneNumber = phoneNumber});
		}

		//
		// POST: /Manage/VerifyPhoneNumber
		[HttpPost("VerifyPhoneNumber")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var user = await GetCurrentUserAsync();
			if (user != null)
			{
				var result = await _userManager.ChangePhoneNumberAsync(user, model.PhoneNumber, model.Code);
				if (result.Succeeded)
				{
					await _signInManager.SignInAsync(user, isPersistent: false);
					SendNotification(nameof(Security), "Settings", NotificationType.Success,
						"Verify phone number success");
					return RedirectToAction(nameof(Security));
				}
			}

			// If we got this far, something failed, redisplay the form
			ModelState.AddModelError(string.Empty, "Failed to verify phone number");
			return View(model);
		}

		//
		// GET: /Manage/RemovePhoneNumber
		[HttpPost("RemovePhoneNumber")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> RemovePhoneNumber()
		{
			var user = await GetCurrentUserAsync();
			if (user != null)
			{
				var result = await _userManager.SetPhoneNumberAsync(user, null);
				if (result.Succeeded)
				{
					await _signInManager.SignInAsync(user, isPersistent: false);
					return RedirectToAction(nameof(Profile), new {Message = ManageMessageId.RemovePhoneSuccess});
				}
			}

			return RedirectToAction(nameof(Profile), new {Message = ManageMessageId.Error});
		}

		[HttpGet("Security")]
		public async Task<IActionResult> Security()
		{
			var model = await BuildSecurityViewModelAsync();

			if (model == null)
			{
				return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
			}

			return View(model);
		}

		private async Task<SecurityViewModel> BuildSecurityViewModelAsync()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return null;
			}

			var model = new SecurityViewModel
			{
				HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null,
				Is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user),
				IsMachineRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user),
				RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user),
				HasPassword = !string.IsNullOrWhiteSpace(user.PasswordHash),
				PhoneNumber = user.PhoneNumber
			};

			return model;
		}

		//GET: /Manage/ManageLogins
		[HttpGet("ExternalLogins")]
		public async Task<IActionResult> ExternalLogins()
		{
			var user = await GetCurrentUserAsync();
			if (user == null)
			{
				return View("Error");
			}

			var userLogins = await _userManager.GetLoginsAsync(user);
			var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();
			var otherLogins = schemes.Where(auth => userLogins.All(ul => auth.Name != ul.LoginProvider)).ToList();
			ViewData["ShowRemoveButton"] = user.PasswordHash != null || userLogins.Count > 1;
			return View(new ManageLoginsViewModel {CurrentLogins = userLogins, OtherLogins = otherLogins});
		}

		//
		// POST: /Manage/LinkLogin
		[HttpPost("LinkLogin")]
		[ValidateAntiForgeryToken]
		public IActionResult LinkLogin(string provider)
		{
			// Request a redirect to the external login provider to link a login for the current user
			var redirectUrl = Url.Action("LinkLoginCallback", "Settings");
			var properties =
				_signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl,
					_userManager.GetUserId(User));
			return Challenge(properties, provider);
		}

		//
		// GET: /Manage/LinkLoginCallback
		[HttpGet("LinkLoginCallback")]
		public async Task<ActionResult> LinkLoginCallback()
		{
			var user = await GetCurrentUserAsync();
			if (user == null)
			{
				return View("Error");
			}

			var info = await _signInManager.GetExternalLoginInfoAsync(await _userManager.GetUserIdAsync(user));
			if (info == null)
			{
				return RedirectToAction(nameof(ExternalLogins), new {Message = ManageMessageId.Error});
			}

			var result = await _userManager.AddLoginAsync(user, info);
			var message = result.Succeeded ? ManageMessageId.AddLoginSuccess : ManageMessageId.Error;
			return RedirectToAction(nameof(ExternalLogins), new {Message = message});
		}

		[HttpGet("Account")]
		public IActionResult Account()
		{
			return View();
		}

		[HttpGet("Applications")]
		public async Task<ActionResult> Applications()
		{
			var grants = await _interaction.GetAllUserConsentsAsync();

			var list = new List<GrantViewModel>();
			foreach (var grant in grants)
			{
				var client = await _clientStore.FindClientByIdAsync(grant.ClientId);
				if (client != null)
				{
					var resources = await _resources.FindResourcesByScopeAsync(grant.Scopes);

					var item = new GrantViewModel
					{
						ClientId = client.ClientId,
						ClientName = client.ClientName ?? client.ClientId,
						ClientLogoUrl = client.LogoUri,
						ClientUrl = client.ClientUri,
						Created = grant.CreationTime,
						Expires = grant.Expiration,
						IdentityGrantNames =
							resources.IdentityResources.Select(x => x.DisplayName ?? x.Name).ToArray(),
						ApiGrantNames = resources.ApiResources.Select(x => x.DisplayName ?? x.Name).ToArray()
					};

					list.Add(item);
				}
			}

			return View(new OAuthApplicationViewModel {Applications = list});
		}

		[HttpPut("Head")]
		public async Task<IActionResult> Head()
		{
			var userId = _userManager.GetUserId(HttpContext.User);
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return NotFound();
			}

			using var reader = new StreamReader(HttpContext.Request.Body);

			var base64 = await reader.ReadToEndAsync();
			base64 = base64.Replace("data:image/png;base64,", "").Replace("data:image/jgp;base64,", "")
				.Replace("data:image/jpg;base64,", "").Replace("data:image/jpeg;base64,", "");
			var bytes = Convert.FromBase64String(base64);

			var intervalPath = $"image/head/{DateTime.Today:yyyyMMdd}/{userId}.png";
			var path = $"wwwroot/{intervalPath}";

			var directory = Path.GetDirectoryName(path);
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}

			await System.IO.File.WriteAllBytesAsync(path, bytes);

			var claims = await _userManager.GetClaimsAsync(user);
			var pictureClaim = claims.FirstOrDefault(x => x.Type == "picture");
			if (pictureClaim == null)
			{
				await _userManager.AddClaimAsync(user, new Claim("picture", intervalPath));
			}
			else
			{
				await _userManager.ReplaceClaimAsync(user, pictureClaim, new Claim("picture", intervalPath));
			}

			return Ok(intervalPath);
		}

		[HttpGet("PersonalData")]
		public async Task<IActionResult> PersonalData()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
			}

			_logger.LogInformation("User with ID '{UserId}' asked for their personal data.",
				_userManager.GetUserId(User));

			// Only include personal data for download
			var personalData = new Dictionary<string, string>();
			var personalDataProps = typeof(IdentityUser).GetProperties().Where(
				prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));
			foreach (var p in personalDataProps)
			{
				personalData.Add(p.Name, p.GetValue(user)?.ToString() ?? "null");
			}

			var logins = await _userManager.GetLoginsAsync(user);
			foreach (var l in logins)
			{
				personalData.Add($"{l.LoginProvider} external login provider key", l.ProviderKey);
			}

			personalData.Add("Authenticator Key", await _userManager.GetAuthenticatorKeyAsync(user));

			Response.Headers.Add("Content-Disposition", "attachment; filename=PersonalData.json");
			return new FileContentResult(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(personalData)),
				"text/json");
		}

		[HttpGet("GenerateRecoveryCodes")]
		public async Task<IActionResult> GenerateRecoveryCodes()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
			}

			var isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
			if (!isTwoFactorEnabled)
			{
				var userId = await _userManager.GetUserIdAsync(user);
				throw new InvalidOperationException(
					$"Cannot generate recovery codes for user with ID '{userId}' because they do not have 2FA enabled.");
			}

			return View(new GenerateRecoveryCodesViewModel());
		}

		[HttpPost("GenerateRecoveryCodes")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> GenerateRecoveryCodes(GenerateRecoveryCodesViewModel model)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
			}

			var isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
			var userId = await _userManager.GetUserIdAsync(user);
			if (!isTwoFactorEnabled)
			{
				throw new InvalidOperationException(
					$"Cannot generate recovery codes for user with ID '{userId}' as they do not have 2FA enabled.");
			}

			var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
			model.RecoveryCodes = recoveryCodes.ToArray();
			model.Message = "You have generated new recovery codes.";

			_logger.LogInformation("User with ID '{UserId}' has generated new 2FA recovery codes.", userId);

			return View("showrecoverycodes",
				new ShowRecoveryCodesModel {RecoveryCodes = model.RecoveryCodes, Message = model.Message});
		}

		[HttpGet("EnableAuthenticator")]
		public async Task<IActionResult> EnableAuthenticator()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return NotFound();
			}

			var model = new EnableAuthenticatorViewModel();
// Load the authenticator key & QR code URI to display on the form
			var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
			if (string.IsNullOrEmpty(unformattedKey))
			{
				await _userManager.ResetAuthenticatorKeyAsync(user);
				unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
			}

			model.SharedKey = FormatKey(unformattedKey);
			var email = await _userManager.GetEmailAsync(user);
			model.AuthenticatorUri = GenerateQrCodeUri(email, unformattedKey);
			return View(model);
		}

		[HttpPost("EnableAuthenticator")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EnableAuthenticator(EnableAuthenticatorViewModel model)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
			}

			if (!ModelState.IsValid)
			{
				return View(nameof(EnableAuthenticator), model);
			}

			// Strip spaces and hypens
			var verificationCode = model.Input.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

			var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
				user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

			if (!is2faTokenValid)
			{
				SendNotification(nameof(EnableAuthenticator), "Settings", NotificationType.Error,
					"Verification code is invalid.");
				return RedirectToAction(nameof(EnableAuthenticator));
			}

			await _userManager.SetTwoFactorEnabledAsync(user, true);
			var userId = await _userManager.GetUserIdAsync(user);
			_logger.LogInformation("User with ID '{UserId}' has enabled 2FA with an authenticator app.", userId);

			if (await _userManager.CountRecoveryCodesAsync(user) == 0)
			{
				var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
				model.RecoveryCodes = recoveryCodes.ToArray();
				SendNotification(nameof(EnableAuthenticator), "Settings", NotificationType.Success,
					"Your authenticator app has been verified.");
				return View("ShowRecoveryCodes", new ShowRecoveryCodesModel {RecoveryCodes = model.RecoveryCodes});
			}

			SendNotification(nameof(Security), "Settings", NotificationType.Success,
				"Your authenticator app has been verified.");
			return RedirectToAction(nameof(Security));
		}

		[HttpGet("ShowRecoveryCodes")]
		public IActionResult ShowRecoveryCodes()
		{
			return View();
		}

		[HttpGet("ResetAuthenticator")]
		public IActionResult ResetAuthenticator()
		{
			return View();
		}

		[HttpPost("ResetAuthenticator")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ResetAuthenticator(ResetAuthenticatorModel model)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
			}

			await _userManager.SetTwoFactorEnabledAsync(user, false);
			await _userManager.ResetAuthenticatorKeyAsync(user);
			var userId = await _userManager.GetUserIdAsync(user);
			_logger.LogInformation("User with ID '{UserId}' has reset their authentication app key.", userId);

			await _signInManager.RefreshSignInAsync(user);
			SendNotification(nameof(ExternalLogins), "Settings", NotificationType.Success,
				"Your authenticator app key has been reset, you will need to configure your authenticator app using the new key.");
			return RedirectToAction(nameof(ExternalLogins));
		}

		[HttpGet("Email")]
		public async Task<IActionResult> Email()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return NotFound();
			}

			var viewModel = new EmailViewModel {Email = user.Email, EmailConfirmed = user.EmailConfirmed};
			return View(viewModel);
		}

		/// <summary>
		/// Handle post back to revoke a client
		/// </summary>
		[HttpPost("Revoke")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Revoke(string clientId)
		{
			await _interaction.RevokeUserConsentAsync(clientId);
			await _events.RaiseAsync(new GrantsRevokedEvent(User.GetSubjectId(), clientId));
			return RedirectToAction(nameof(Applications));
		}

		#region Helpers

		private void AddErrors(IdentityResult result)
		{
			foreach (var error in result.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}
		}

		private enum ManageMessageId
		{
			AddLoginSuccess,
			RemovePhoneSuccess,
			Error
		}

		private Task<IdentityUser> GetCurrentUserAsync()
		{
			return _userManager.GetUserAsync(HttpContext.User);
		}

		private string FormatKey(string unformattedKey)
		{
			var result = new StringBuilder();
			var currentPosition = 0;
			while (currentPosition + 4 < unformattedKey.Length)
			{
				result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
				currentPosition += 4;
			}

			if (currentPosition < unformattedKey.Length)
			{
				result.Append(unformattedKey.Substring(currentPosition));
			}

			return result.ToString().ToLowerInvariant();
		}

		private string GenerateQrCodeUri(string email, string unformattedKey)
		{
			return string.Format(
				"otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6",
				_urlEncoder.Encode(_options.SiteName),
				_urlEncoder.Encode(email),
				unformattedKey);
		}

		#endregion
	}
}
