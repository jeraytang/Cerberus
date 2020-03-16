using System;

namespace IdentityServer4.Admin.Common
{
	public static class StringExtensions
	{
		public static T ToEnum<T>(this string value)
		{
			var result = (T)Enum.Parse(typeof(T), value, true);
			return result;
		}
	}
}
