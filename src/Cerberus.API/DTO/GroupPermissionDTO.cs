using System.Collections.Generic;

namespace Cerberus.API.DTO
{
	public class GroupPermissionDTO
	{
		public string Module { get; set; }

		public List<GrantPermissionDTO> Permissions { get; set; }
	}
}
