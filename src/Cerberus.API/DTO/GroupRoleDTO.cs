using System.Collections.Generic;

namespace Cerberus.API.DTO
{
	public class GroupRoleDTO
	{
		public string Type { get; set; }

		public List<GrantRoleDTO> Roles { get; set; }
	}
}
