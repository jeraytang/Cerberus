using System;
using System.ComponentModel.DataAnnotations;

namespace IdentityServer4.Admin.DTO
{
	public class SecretDTO
	{
		public Guid Id { get; set; } = Guid.NewGuid();

		/// <summary>
		/// Gets or sets the description.
		/// </summary>
		/// <value>
		/// The description.
		/// </value>
		[Required]
		[StringLength(100)]
		public string Description { get; set; }

		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <value>
		/// The value.
		/// </value>
		[Required]
		[StringLength(36)]
		public string Value { get; set; }

		/// <summary>
		/// Gets or sets the expiration.
		/// </summary>
		/// <value>
		/// The expiration.
		/// </value>
		public string Expiration { get; set; }

		/// <summary>
		/// Gets or sets the type of the client secret.
		/// </summary>
		/// <value>
		/// The type of the client secret.
		/// </value>
		[Required]
		public string Type { get; set; }
	}
}
