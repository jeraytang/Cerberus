using System;
using IdentityServer4.SecurityTokenService.Models;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace IdentityServer4.SecurityTokenService.Extensions
{
	public static class ViewDataDictionaryExtensions
	{
		public static NotificationViewModel GetNotification(this ViewDataDictionary viewData)
		{
			return viewData["__notification"] as NotificationViewModel;
		}

		public static void SetNotification(this ViewDataDictionary viewData, NotificationViewModel model)
		{
			viewData["__notification"] = model ?? throw new ArgumentNullException(nameof(model));
		}
	}
}
