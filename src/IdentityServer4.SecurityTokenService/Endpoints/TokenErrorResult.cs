using System;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using IdentityServer4.ResponseHandling;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IdentityServer4.SecurityTokenService.Endpoints
{
	internal class TokenErrorResult : IEndpointResult
	{
		public TokenErrorResponse Response { get; }

		public TokenErrorResult(TokenErrorResponse error)
		{
			if (string.IsNullOrWhiteSpace(error.Error))
			{
				throw new ArgumentNullException(nameof(error.Error), "Error must be set");
			}

			Response = error;
		}

		public async Task ExecuteAsync(HttpContext context)
		{
			context.Response.StatusCode = 400;
			context.Response.SetNoCache();

			var dto = new ResultDto {error = Response.Error, error_description = Response.ErrorDescription};

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
			public string error { get; set; }
			public string error_description { get; set; }
		}
	}
}
