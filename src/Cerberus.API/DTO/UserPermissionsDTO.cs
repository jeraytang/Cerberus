using System.Collections.Generic;

namespace Cerberus.API.DTO
{
	public class UserPermissionsDTO
	{
		public string UserName { get; set; }
		public string Email { get; set; }
		public string Name { get; set; }

		public List<ServicePermissionsDTO> Services { get; set; }
	}
}
