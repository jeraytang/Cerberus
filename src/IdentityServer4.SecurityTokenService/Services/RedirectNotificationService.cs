using System;
using IdentityServer4.SecurityTokenService.Models;
using Microsoft.Extensions.Caching.Memory;

namespace IdentityServer4.SecurityTokenService.Services
{
	public class RedirectNotificationService : IRedirectNotificationService
	{
		private readonly IMemoryCache _memoryCache;

		public RedirectNotificationService(IMemoryCache memoryCache)
		{
			_memoryCache = memoryCache;
		}

		public NotificationViewModel Receive(string location)
		{
			var model = _memoryCache.Get<NotificationViewModel>(location);
			if (model != null)
			{
				// 消息只显示一次
				_memoryCache.Remove(location);
			}
			return model;
		}

		public void Send(string location, NotificationViewModel viewModel)
		{
			_memoryCache.Set(location, viewModel, DateTimeOffset.Now.AddMinutes(30));
		}
	}
}
