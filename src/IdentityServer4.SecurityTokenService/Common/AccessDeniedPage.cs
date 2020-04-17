using System.Net.Http;

namespace IdentityServer4.SecurityTokenService.Common
{
    public class AccessDeniedPage
    {
        public string HttpMethod { get; set; }

        public string Controller { get; set; }

        public string Action { get; set; }
    }
}