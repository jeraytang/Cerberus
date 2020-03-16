using Microsoft.Extensions.Configuration;

namespace IdentityServer4.Admin.Common
{
    public class AdminOptions
    {
        private readonly IConfiguration _configuration;

        public AdminOptions(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string ClientId => string.IsNullOrWhiteSpace(_configuration["ClientId"])
            ? string.Empty
            : _configuration["ClientId"];

        public string ClientSecret => _configuration["ClientSecret"];

        public string Authority => string.IsNullOrWhiteSpace(_configuration["Authority"])
            ? string.Empty
            : _configuration["Authority"];

        public string Secret => string.IsNullOrWhiteSpace(_configuration["Secret"])
            ? string.Empty
            : _configuration["Secret"];

        public bool RequireHttpsMetadata => !string.IsNullOrWhiteSpace(_configuration["RequireHttpsMetadata"]) &&
                                            bool.Parse(_configuration["RequireHttpsMetadata"]);
    }
}