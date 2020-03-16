using System;
using System.Text;
using IdentityServer4.Extensions;
using IdentityServer4.SecurityTokenService.Extensions;
using IdentityServer4.SecurityTokenService.Models;
using IdentityServer4.SecurityTokenService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IdentityServer4.SecurityTokenService.Controllers
{
	public class BaseController : Controller
	{
		protected IRedirectNotificationService RedirectNotificationService { get; private set; }

		protected BaseController(IRedirectNotificationService redirectNotificationService)
		{
			RedirectNotificationService = redirectNotificationService;
		}

		protected void SendNotification(string action, string controllerName,
			NotificationViewModel notificationViewModel)
		{
			if (string.IsNullOrWhiteSpace(controllerName))
			{
				throw new ArgumentNullException(nameof(controllerName));
			}

			if (string.IsNullOrWhiteSpace(action))
			{
				throw new ArgumentNullException(nameof(action));
			}

			if (notificationViewModel == null)
			{
				throw new ArgumentNullException(nameof(notificationViewModel));
			}

			var subjectId = GetSubjectId();
			if (string.IsNullOrWhiteSpace(subjectId))
			{
				return;
			}

			controllerName = controllerName.ToLower();
			var controllerEndsWidth = "controller";
			if (controllerName.EndsWith(controllerEndsWidth))
			{
				controllerName = controllerName.Substring(0, controllerName.Length - controllerEndsWidth.Length);
			}

			RedirectNotificationService.Send($"{subjectId}/{controllerName}/{action.ToLower()}",
				notificationViewModel);
		}

		protected void SendNotification(string action, string controllerName, NotificationType type, string message)
		{
			SendNotification(action, controllerName, new NotificationViewModel(type, message));
		}

		protected void SendNotificationFromModelErrors(string action, string controllerName)
		{
			var messageBuilder = new StringBuilder();
			foreach (var stateValue in ModelState.Values)
			{
				foreach (var error in stateValue.Errors)
				{
					messageBuilder.Append($"{error.ErrorMessage}, ");
				}
			}

			var message = messageBuilder.ToString();
			if (message.Length > 2)
			{
				message = message.Substring(0, message.Length - 2);
			}

			SendNotification(action, controllerName, new NotificationViewModel(NotificationType.Error, message));
		}

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			if (context.ActionDescriptor is ControllerActionDescriptor action)
			{
				var subjectId = GetSubjectId();
				if (!string.IsNullOrWhiteSpace(subjectId))
				{
					var target = $"{subjectId}/{action.ControllerName}/{action.ActionName}".ToLower();
					var notification = RedirectNotificationService.Receive(target);
					if (notification != null)
					{
						ViewData.SetNotification(notification);
					}
				}
			}

			base.OnActionExecuting(context);
		}

		protected string GetSubjectId()
		{
			if (HttpContext.User != null && HttpContext.User.HasClaim(x => x.Type == "sub"))
			{
				return HttpContext.User.GetSubjectId();
			}

			return null;
		}
	}
}
