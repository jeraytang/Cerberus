// using System;
// using System.ComponentModel.DataAnnotations;
// using Microsoft.AspNetCore.Identity;
//
// namespace IdentityServer4.SecurityTokenService.Data
// {
// 	public class User : IdentityUser
// 	{
// 		/// <summary>
// 		/// 用户名
// 		/// </summary>
// 		[StringLength(200)]
// 		[Required]
// 		[ProtectedPersonalData]
// 		public override string UserName { get; set; }
//
// 		/// <summary>
// 		/// The normalized userName
// 		/// </summary>
// 		[StringLength(200)]
// 		public override string NormalizedUserName { get; set; }
//
// 		/// <summary>
// 		/// 邮箱
// 		/// </summary>
// 		[ProtectedPersonalData]
// 		[StringLength(200)]
// 		public override string Email { get; set; }
//
// 		/// <summary>
// 		/// Gets or sets the normalized email address for this user.
// 		/// </summary>
// 		[StringLength(200)]
// 		public override string NormalizedEmail { get; set; }
//
// 		/// <summary>
// 		/// Gets or sets a salted and hashed representation of the password for this user.
// 		/// </summary>
// 		[StringLength(512)]
// 		public override string PasswordHash { get; set; }
//
// 		/// <summary>
// 		/// A random value that must change whenever a users credentials change (password changed, login removed)
// 		/// </summary>
// 		[StringLength(36)]
// 		public override string SecurityStamp { get; set; }
//
// 		/// <summary>
// 		/// A random value that must change whenever a user is persisted to the store
// 		/// </summary>
// 		[StringLength(36)]
// 		public override string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
//
// 		/// <summary>Gets or sets a telephone number for the user.</summary>
// 		[ProtectedPersonalData]
// 		[StringLength(200)]
// 		public override string PhoneNumber { get; set; }
// 	}
// }
