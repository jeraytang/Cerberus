using System.Collections.Generic;

namespace Cerberus.API.DTO
{
	public class ServicePermissionsDTO
	{
		public string Service { get; set; }

		public List<GroupPermissionDTO> Groups { get; set; }
	}
}
