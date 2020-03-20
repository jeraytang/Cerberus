using System.ComponentModel.DataAnnotations;

namespace Cerberus.API.DTO
{
    public class UserDTO
    {
        public string Id { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        [StringLength(36)]
        [Required]
        public string UserName { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        [StringLength(200)]
        [EmailAddress]
        [Required]
        public string Email { get; set; }

        /// <summary>
        /// 部门
        /// </summary>
        [StringLength(100)]
        public string Department { get; set; } = "";

        /// <summary>
        /// 传真
        /// </summary>
        [StringLength(100)]
        public string Fax { get; set; } = "";

        /// <summary>
        /// 职位
        /// </summary>
        [StringLength(100)]
        public string Title { get; set; } = "";

        /// <summary>
        /// 座机电话
        /// </summary>
        [StringLength(20)]
        public string Telephone { get; set; } = "";

        /// <summary>
        /// 生日
        /// </summary>
        public string Birthday { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        public string Sex { get; set; } = "Unknown";

        /// <summary>
        /// BIO
        /// </summary>
        [StringLength(200)]
        public string Bio { get; set; } = "";

        /// <summary>
        /// 名
        /// </summary>
        [StringLength(200)]
        public string GivenName { get; set; } = "";

        /// <summary>
        /// 姓
        /// </summary>
        [StringLength(200)]
        public string FamilyName { get; set; } = "";

        /// <summary>
        /// 昵称
        /// </summary>
        [StringLength(200)]
        public string NickName { get; set; } = "";

        /// <summary>
        /// 个人网址
        /// </summary>
        [StringLength(500)]
        public string Website { get; set; } = "";

        /// <summary>
        /// 公司
        /// </summary>
        [StringLength(500)]
        public string Company { get; set; } = "";

        /// <summary>
        /// 所在地
        /// </summary>
        [StringLength(200)]
        public string Location { get; set; } = "";

        /// <summary>
        ///手机号码
        /// </summary>
        [StringLength(25)]
        public string PhoneNumber { get; set; } = "";

        /// <summary>
        /// 用户来源
        /// </summary>
        [StringLength(20)]
        public string Source { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public bool Enabled { get; set; }
    }
}