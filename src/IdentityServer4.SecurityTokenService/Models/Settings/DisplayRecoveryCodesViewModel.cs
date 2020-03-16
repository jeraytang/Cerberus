using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IdentityServer4.SecurityTokenService.Models.Settings
{
	public class DisplayRecoveryCodesViewModel
	{
		/// <summary>
		/// Recovery codes
		/// </summary>
		[Required]
		public IEnumerable<string> Codes { get; set; }
	}
}
