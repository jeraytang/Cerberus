using System;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using IdentityServer4.ResponseHandling;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IdentityServer4.SecurityTokenService.Endpoints
{
	internal class TokenResult : IEndpointResult
	{
		public TokenResponse Response { get; set; }

		public TokenResult(TokenResponse response)
		{
			Response = response ?? throw new ArgumentNullException(nameof(response));
		}

		public async Task ExecuteAsync(HttpContext context)
		{
			context.Response.SetNoCache();

			var dto = new ResultDto
			{
				id_token = Response.IdentityToken,
				access_token = Response.AccessToken,
				refresh_token = Response.RefreshToken,
				expires_in = Response.AccessTokenLifetime,
				token_type = OidcConstants.TokenResponse.BearerTokenType,
				scope = Response.Scope
			};

			if (Response.Custom.IsNullOrEmpty())
			{
				await context.Response.WriteJsonAsync(dto);
			}
			else
			{
				var jobject = JObject.FromObject(dto,
					new JsonSerializer()
					{
						DefaultValueHandling = DefaultValueHandling.Ignore,
						NullValueHandling = NullValueHandling.Ignore
					});
				jobject.AddDictionary(Response.Custom);

				await context.Response.WriteJsonAsync(jobject);
			}
		}

		internal class ResultDto
		{
			public string id_token { get; set; }
			public string access_token { get; set; }
			public int expires_in { get; set; }
			public string token_type { get; set; }
			public string refresh_token { get; set; }
			public string scope { get; set; }
		}
	}
}
