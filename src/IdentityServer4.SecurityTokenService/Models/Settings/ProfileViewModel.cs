using System.ComponentModel.DataAnnotations;
using IdentityServer4.SecurityTokenService.Data;

namespace IdentityServer4.SecurityTokenService.Models.Settings
{
	public class ProfileInputModel
	{
		[StringLength(32)] public string NickName { get; set; }

		public Sex Sex { get; set; }

		[StringLength(10)]
		public string Birthday { get; set; }

		/// <summary>
		/// 名
		/// </summary>
		[StringLength(24)]
		public string GivenName { get; set; }

		[StringLength(24)] public string Telephone { get; set; }

		/// <summary>
		/// 姓
		/// </summary>
		[StringLength(24)]
		public string FamilyName { get; set; }

		[StringLength(200)] public string Bio { get; set; }

		[StringLength(500)] [Url] public string Website { get; set; }

		[StringLength(100)] public string Company { get; set; }

		[StringLength(200)] public string Location { get; set; }
	}

	public class ProfileViewModel : ProfileInputModel
	{
		[StringLength(200)] public string Picture { get; set; }
	}
}
