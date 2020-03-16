using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer4.SecurityTokenService.Data
{
	public class AutoMapperProfile : Profile
	{
		public AutoMapperProfile()
		{
			CreateMap<IdentityOptions, IdentityOptions>();
		}
	}
}
