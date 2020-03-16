using AutoMapper;
using Cerberus.API.Data;
using Microsoft.AspNetCore.Identity;

namespace Cerberus.API.DTO
{
	public class AutoMapperProfile : Profile
	{
		public AutoMapperProfile()
		{
			CreateMap<IdentityOptions, IdentityOptions>();

			CreateMap<Permission, ListPermissionDTO>()
				.ForMember(x => x.Service, opt =>
					opt.MapFrom(x => x.Service.Name));
			CreateMap<Permission, PermissionDTO>();

			CreateMap<Role, ListRoleDTO>();
			CreateMap<Role, RoleDTO>();

			CreateMap<Service, ListServiceDTO>();
			CreateMap<Service, ServiceDTO>();
		}
	}
}
