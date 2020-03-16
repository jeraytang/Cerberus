using Microsoft.Extensions.Configuration;

namespace Cerberus.API.Common
{
	public class CerberusOptions
	{
		private readonly IConfiguration _configuration;

		public CerberusOptions(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public string Audience => _configuration["Audience"];

		public string Authority => _configuration["Authority"];

		public bool RequireHttpsMetadata => bool.Parse(_configuration["RequireHttpsMetadata"]);

		public string ConnectionString => _configuration["ConnectionString"];

		public string AdminEmail => _configuration["AdminEmail"];

		public string AdminPassword => _configuration["AdminPassword"];

		public string ApiSecret => _configuration["ApiSecret"];

		public string TestData => _configuration["TestData"];
	}
}
