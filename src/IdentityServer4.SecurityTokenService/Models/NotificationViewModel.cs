using System;

namespace IdentityServer4.SecurityTokenService.Models
{
	public class NotificationViewModel
	{
		/// <summary>
		/// 通知类型
		/// </summary>
		public NotificationType Type { get; set; }

		/// <summary>
		/// 通知消息
		/// </summary>
		public string Message { get; set; }

		public NotificationViewModel(NotificationType type, string message)
		{
			if (string.IsNullOrWhiteSpace(message))
			{
				throw new ArgumentNullException(nameof(message));
			}

			Type = type;
			Message = message;
		}
	}

	public enum NotificationType
	{
		Warning,
		Success,
		Error,
		Info
	}
}
