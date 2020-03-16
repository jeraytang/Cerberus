using System.Linq;
using AutoMapper;
using IdentityServer4.Storage.Adapter.Entities;
using IdentityServer4.Storage.Adapter.Extensions;
using IdentityServer4.Stores.Serialization;
using Newtonsoft.Json;

namespace IdentityServer4.Storage.Adapter
{
	public class AutoMapperProfile : Profile
	{
		public AutoMapperProfile()
		{
			#region Client

			CreateMap<IdentityServer4.Models.Client, Client>()
				.ForMember(
					x => x.Claims,
					opt => opt.MapFrom(x => x.Claims == null
						? null
						: JsonConvert.SerializeObject(x.Claims.Select(y => new ClaimLite
						{
							Type = y.Type, Value = y.Value, ValueType = y.ValueType
						}))))
				.ForMember(x => x.Properties,
					opt => opt.MapFrom(x => x.Properties == null
						? null
						: JsonConvert.SerializeObject(x.Properties)))
				.ForMember(x => x.AllowedScopes,
					opt => opt.MapFrom(x => x.AllowedScopes == null
						? null
						: string.Join(" ", x.AllowedScopes)))
				.ForMember(x => x.ClientSecrets, opt => opt.MapFrom(x => x.ClientSecrets == null
					? null
					: JsonConvert.SerializeObject(x.ClientSecrets.Select(y => new Secret
					{
						Value = y.Value, Description = y.Description, Type = y.Type, Expiration = y.Expiration
					}))))
				.ForMember(x => x.RedirectUris, opt => opt.MapFrom(x => x.RedirectUris == null
					? null
					: string.Join('\n', x.RedirectUris)))
				.ForMember(x => x.AllowedGrantTypes, opt => opt.MapFrom(x => x.AllowedGrantTypes == null
					? null
					: string.Join(' ', x.AllowedGrantTypes)))
				.ForMember(x => x.IdentityProviderRestrictions, opt => opt.MapFrom(x =>
					x.IdentityProviderRestrictions == null
						? null
						: string.Join(' ', x.IdentityProviderRestrictions)))
				.ForMember(x => x.PostLogoutRedirectUris, opt => opt.MapFrom(x => x.PostLogoutRedirectUris == null
					? null
					: string.Join('\n', x.PostLogoutRedirectUris)))
				.ForMember(x => x.AllowedCorsOrigins, opt => opt.MapFrom(x => x.AllowedCorsOrigins == null
					? null
					: string.Join('\n', x.AllowedCorsOrigins)));

			CreateMap<Client, IdentityServer4.Models.Client>()
				.ForMember(x => x.Claims,
					opt => opt.MapFrom(x => x.Claims.ToClaims()))
				.ForMember(x => x.Properties,
					opt => opt.MapFrom(x => x.Properties.ToProperties()))
				.ForMember(x => x.AllowedScopes,
					opt => opt.MapFrom(x => x.AllowedScopes.ToHashsetBySeparator(' ')))
				.ForMember(x => x.ClientSecrets,
					opt => opt.MapFrom(x => x.ClientSecrets.ToSecrets()))
				.ForMember(x => x.RedirectUris,
					opt => opt.MapFrom(x => x.RedirectUris.ToUrls()))
				.ForMember(x => x.AllowedGrantTypes,
					opt => opt.MapFrom(x => x.AllowedGrantTypes.ToHashsetBySeparator(' ')))
				.ForMember(x => x.IdentityProviderRestrictions,
					opt => opt.MapFrom(x => x.IdentityProviderRestrictions.ToHashsetBySeparator(' ')))
				.ForMember(x => x.PostLogoutRedirectUris, opt =>
					opt.MapFrom(x => x.PostLogoutRedirectUris.ToUrls()))
				.ForMember(x => x.AllowedCorsOrigins, opt => opt.MapFrom(x => x.AllowedCorsOrigins == null
					? null
					: x.AllowedCorsOrigins.ToHashsetBySeparator('\n')));

			#endregion

			#region ApiResource

			CreateMap<IdentityServer4.Models.ApiResource, ApiResource>()
				.ForMember(x => x.Properties,
					opt => opt.MapFrom(x => x.Properties == null
						? null
						: JsonConvert.SerializeObject(x.Properties)))
				.ForMember(x => x.ApiSecrets, opt => opt.MapFrom(x => x.ApiSecrets == null
					? null
					: JsonConvert.SerializeObject(x.ApiSecrets.Select(y => new Secret
					{
						Value = y.Value, Description = y.Description, Type = y.Type, Expiration = y.Expiration
					}))))
				.ForMember(x => x.UserClaims, opt => opt.MapFrom(x => x.UserClaims == null
					? null
					: string.Join(' ', x.UserClaims)));

			CreateMap<ApiResource, IdentityServer4.Models.ApiResource>()
				.ForMember(x => x.ApiSecrets,
					opt => opt.MapFrom(x => x.ApiSecrets.ToSecrets()))
				.ForMember(x => x.Properties,
					opt => opt.MapFrom(x => x.Properties.ToProperties()))
				.ForMember(x => x.UserClaims,
					opt => opt.MapFrom(x => x.UserClaims.ToHashsetBySeparator(' ')));

			#endregion

			#region Scope

			CreateMap<ApiScope, Models.Scope>().ForMember(x => x.UserClaims,
				opt => opt.MapFrom(x => x.UserClaims.ToHashsetBySeparator(' ')));

			#endregion

			#region IdentityResource

			CreateMap<IdentityServer4.Models.IdentityResource, IdentityResource>()
				.ForMember(x => x.Properties,
					opt => opt.MapFrom(x => x.Properties == null
						? null
						: JsonConvert.SerializeObject(x.Properties)))
				.ForMember(x => x.UserClaims, opt => opt.MapFrom(x => x.UserClaims == null
					? null
					: string.Join(' ', x.UserClaims)));

			CreateMap<IdentityResource, IdentityServer4.Models.IdentityResource>()
				.ForMember(x => x.Properties,
					opt => opt.MapFrom(x => x.Properties.ToProperties()))
				.ForMember(x => x.UserClaims,
					opt => opt.MapFrom(x => x.UserClaims.ToHashsetBySeparator(' ')));

			#endregion
		}
	}
}
