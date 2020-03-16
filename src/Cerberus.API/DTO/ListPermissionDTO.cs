namespace Cerberus.API.DTO
{
	public class ListPermissionDTO
	{
		public string Id { get; set; }

		public string Service { get; set; }

		public string Module { get; set; }

		public string Name { get; set; }

		public string Type { get; set; }

		public string Identification { get; set; }

		public string Description { get; set; }

		public bool Expired { get; set; }
	}
}
