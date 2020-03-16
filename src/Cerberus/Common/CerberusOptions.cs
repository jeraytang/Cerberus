using Microsoft.Extensions.Configuration;

namespace Cerberus.Common
{
    public class CerberusOptions
    {
        private readonly IConfiguration _configuration;

        public CerberusOptions(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string ClientId => _configuration["ClientId"];

        public string ClientSecret => _configuration["ClientSecret"];

        public string Authority => _configuration["Authority"];

        public bool RequireHttpsMetadata => bool.Parse(_configuration["RequireHttpsMetadata"]);

        public string CerberusApi => _configuration["CerberusApi"];
    }
}