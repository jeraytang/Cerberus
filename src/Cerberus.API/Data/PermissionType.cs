namespace Cerberus.API.Data
{
	public enum PermissionType
	{
		/// <summary>
		/// 菜单
		/// </summary>
		Menu = 0,

		/// <summary>
		/// 功能
		/// </summary>
		Function = 1,

		/// <summary>
		/// 其它
		/// </summary>
		Other = 2,

		/// <summary>
		/// 后端 API 权限
		/// </summary>
		Api = 3,

		/// <summary>
		/// UI 动作权限
		/// </summary>
		Action = 4
	}
}
