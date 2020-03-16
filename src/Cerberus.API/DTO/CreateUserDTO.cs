using System.ComponentModel.DataAnnotations;

namespace Cerberus.API.DTO
{
    public class CreateUserDTO
    {
        /// <summary>
        /// Email
        /// </summary>
        [EmailAddress]
        [Required]
        public string Email { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [Required]
        [StringLength(32)]
        public string Password { get; set; }

        /// <summary>
        /// 用户来源
        /// </summary>
        [StringLength(20)]
        public string Source { get; set; }
    }
}