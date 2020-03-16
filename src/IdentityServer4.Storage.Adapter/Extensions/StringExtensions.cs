using System.Collections.Generic;
using System.Linq;
using IdentityServer4.Models;
using IdentityServer4.Stores.Serialization;
using Newtonsoft.Json;

namespace IdentityServer4.Storage.Adapter.Extensions
{
	public static class StringExtensions
	{
		public static IDictionary<string, string> ToProperties(this string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return new Dictionary<string, string>();
			}

			return JsonConvert.DeserializeObject<Dictionary<string, string>>(value);
		}

		public static HashSet<System.Security.Claims.Claim> ToClaims(this string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return new HashSet<System.Security.Claims.Claim>();
			}

			return JsonConvert.DeserializeObject<ClaimLite[]>(value)
				.Select(x => new System.Security.Claims.Claim(x.Type, x.Value, x.ValueType)).ToHashSet();
		}

		public static HashSet<string> ToHashsetBySeparator(this string value, char separator = ' ')
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return new HashSet<string>();
			}

			return value.Split(separator).ToHashSet();
		}

		public static HashSet<string> ToUrls(this string value)
		{
			return value.ToHashsetBySeparator('\n');
		}

		public static HashSet<Secret> ToSecrets(this string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return new HashSet<Secret>();
			}

			return JsonConvert.DeserializeObject<Secret[]>(value).ToHashSet();
		}
	}
}
