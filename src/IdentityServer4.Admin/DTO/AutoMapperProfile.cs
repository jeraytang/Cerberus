using AutoMapper;
using IdentityServer4.Storage.Adapter.Entities;

namespace IdentityServer4.Admin.DTO
{
	public class AutoMapperProfile : Profile
	{
		public AutoMapperProfile()
		{
			CreateMap<Client, ListClientDTO>().ForMember(x => x.Scopes,
					opt => opt.MapFrom(x => x.AllowedScopes.Replace(";", " ")))
				.ForMember(x => x.GrantType,
					opt => opt.MapFrom(x => x.AllowedGrantTypes));

			CreateMap<AddClientDTO, Client>();

			CreateMap<Client, ClientDTO>();

			CreateMap<ClientDTO, Client>();

			CreateMap<AddIdentityResourceDTO, IdentityResource>();

			CreateMap<IdentityResourceDTO, IdentityResource>();

			CreateMap<IdentityResource, IdentityResourceDTO>();

			CreateMap<IdentityResource, ListIdentityResourceDTO>();

			CreateMap<AddApiResourceDTO, IdentityServer4.Storage.Adapter.Entities.ApiResource>();

			CreateMap<ApiResourceDTO, IdentityServer4.Storage.Adapter.Entities.ApiResource>();

			CreateMap<IdentityServer4.Storage.Adapter.Entities.ApiResource, ApiResourceDTO>();

			CreateMap<IdentityServer4.Storage.Adapter.Entities.ApiResource, ListApiResourceDTO>();

			CreateMap<ApiScope, ListApiResourceScopeDTO>();

			CreateMap<AddApiResourceScopeDTO, ApiScope>();

			CreateMap<ApiScope, ApiResourceScopeDTO>();

			CreateMap<ApiResourceScopeDTO, ApiScope>();
		}
	}
}
