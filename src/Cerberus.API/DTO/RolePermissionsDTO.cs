using System.Collections.Generic;

namespace Cerberus.API.DTO
{
	public class RolePermissionsDTO
	{
		public string Role { get; set; }
		public List<ServicePermissionsDTO> Services { get; set; }
	}
}
