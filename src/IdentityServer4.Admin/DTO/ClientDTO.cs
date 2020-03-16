using System;
using System.ComponentModel.DataAnnotations;
using IdentityServer4.Models;

namespace IdentityServer4.Admin.DTO
{
	/// <summary>
	/// 45 properties
	/// </summary>
	public class ClientDTO
	{
		public Guid Id { get; set; }

		public string Enabled { get; set; }

		[Required] [StringLength(100)] public string ClientId { get; set; }

		[Required] [StringLength(25)] public string ProtocolType { get; set; } = "oidc";

		public string RequireClientSecret { get; set; }

		[Required] [StringLength(100)] public string ClientName { get; set; }

		[StringLength(500)] public string Description { get; set; }

		[StringLength(1000)] public string ClientUri { get; set; }

		[StringLength(1000)] public string LogoUri { get; set; }

		public string RequireConsent { get; set; }

		public string AllowRememberConsent { get; set; }

		[Required] [StringLength(120)] public string AllowedGrantTypes { get; set; }

		public string RequirePkce { get; set; }

		public string AllowPlainTextPkce { get; set; }

		public string AllowAccessTokensViaBrowser { get; set; }

		[StringLength(1500)] public string RedirectUris { get; set; }

		[StringLength(1500)] public string PostLogoutRedirectUris { get; set; }

		[StringLength(1000)] public string FrontChannelLogoutUri { get; set; }

		public string FrontChannelLogoutSessionRequired { get; set; }

		[StringLength(1000)] public string BackChannelLogoutUri { get; set; }

		public string BackChannelLogoutSessionRequired { get; set; }

		public string AllowOfflineAccess { get; set; }

		[StringLength(500)] public string AllowedScopes { get; set; }

		public string AlwaysIncludeUserClaimsInIdToken { get; set; }

		public int IdentityTokenLifetime { get; set; } = 300;

		public int AccessTokenLifetime { get; set; } = 3600;

		public int AuthorizationCodeLifetime { get; set; } = 300;

		public int AbsoluteRefreshTokenLifetime { get; set; } = 2592000;

		public int SlidingRefreshTokenLifetime { get; set; } = 1296000;

		public int? ConsentLifetime { get; set; } = null;

		public string RefreshTokenUsage { get; set; } = TokenUsage.OneTimeOnly.ToString();

		public string UpdateAccessTokenClaimsOnRefresh { get; set; }

		public string RefreshTokenExpiration { get; set; } = TokenExpiration.Absolute.ToString();

		public string AccessTokenType { get; set; } = IdentityServer4.Models.AccessTokenType.Jwt.ToString();

		public string EnableLocalLogin { get; set; }

		[StringLength(1000)] public string IdentityProviderRestrictions { get; set; }

		public string IncludeJwtId { get; set; }

		public string AlwaysSendClientClaims { get; set; }

		[StringLength(50)] public string ClientClaimsPrefix { get; set; } = "client_";

		[StringLength(50)] public string PairWiseSubjectSalt { get; set; }

		public int? UserSsoLifetime { get; set; }

		public string UserCodeType { get; set; }

		public int DeviceCodeLifetime { get; set; } = 300;

		[StringLength(1500)] public string AllowedCorsOrigins { get; set; }
	}
}
