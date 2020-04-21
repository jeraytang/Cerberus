using System;
using Microsoft.Extensions.Configuration;

namespace IdentityServer4.SecurityTokenService.Common
{
    public class STSOptions
    {
        private readonly IConfiguration _configuration;

        public STSOptions(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string ConnectionString => _configuration["ConnectionString"];

        public string IdentityConnectionString => _configuration["IdentityConnectionString"];

        public string EmailHost => string.IsNullOrWhiteSpace(_configuration["Email:Host"])
            ? string.Empty
            : _configuration["Email:Host"];

        public string EmailAccount => string.IsNullOrWhiteSpace(_configuration["Email:Account"])
            ? string.Empty
            : _configuration["Email:Account"];

        public string EmailPassword => string.IsNullOrWhiteSpace(_configuration["Email:Password"])
            ? string.Empty
            : _configuration["Email:Password"];

        public string EmailDisplayName => string.IsNullOrWhiteSpace(_configuration["Email:DisplayName"])
            ? string.Empty
            : _configuration["Email:DisplayName"];

        public int EmailPort => string.IsNullOrWhiteSpace(_configuration["Email:Port"])
            ? 25
            : int.Parse(_configuration["Email:Port"]);

        public bool EmailEnableSSL => !string.IsNullOrWhiteSpace(_configuration["Email:EnableSSL"]) &&
                                      bool.Parse(_configuration["Email:EnableSSL"]);

        public bool OpenRegister => !string.IsNullOrWhiteSpace(_configuration["OpenRegister"]) &&
                                    bool.Parse(_configuration["OpenRegister"]);

        public string SiteName => string.IsNullOrWhiteSpace(_configuration["SiteName"])
            ? "IdentityServer4.SecurityTokenService"
            : _configuration["SiteName"];

        public bool AllowLocalLogin => !string.IsNullOrWhiteSpace(_configuration["AllowLocalLogin"]) &&
                                       bool.Parse(_configuration["AllowLocalLogin"]);

        public bool AllowRememberLogin => !string.IsNullOrWhiteSpace(_configuration["AllowRememberLogin"]) &&
                                          bool.Parse(_configuration["AllowRememberLogin"]);

        public TimeSpan RememberMeLoginDuration => string.IsNullOrWhiteSpace(_configuration["RememberMeLoginDuration"])
            ? TimeSpan.FromDays(30)
            : TimeSpan.FromDays(int.Parse(_configuration["RememberMeLoginDuration"]));

        public bool ShowLogoutPrompt => !string.IsNullOrWhiteSpace(_configuration["ShowLogoutPrompt"]) &&
                                        bool.Parse(_configuration["ShowLogoutPrompt"]);

        public bool AutomaticRedirectAfterSignOut =>
            !string.IsNullOrWhiteSpace(_configuration["AutomaticRedirectAfterSignOut"]) &&
            bool.Parse(_configuration["AutomaticRedirectAfterSignOut"]);

        public string WindowsAuthenticationSchemeName =>
            string.IsNullOrWhiteSpace(_configuration["WindowsAuthenticationSchemeName"])
                ? Microsoft.AspNetCore.Server.IISIntegration.IISDefaults.AuthenticationScheme
                : _configuration["WindowsAuthenticationSchemeName"];

        public bool IncludeWindowsGroups => !string.IsNullOrWhiteSpace(_configuration["IncludeWindowsGroups"]) &&
                                            bool.Parse(_configuration["IncludeWindowsGroups"]);

        public string SmsPostUrl => _configuration["SMS:PostUrl"];
        public string SmsAccount => _configuration["SMS:Account"];
        public string SmsPassword => _configuration["SMS:Password"];
        public string SmsCodeTemplate => _configuration["SMS:CodeTemplate"];
        public string LoinLogo => _configuration["LoinPage:Logo"];
        public string LoinSlogan => _configuration["LoinPage:Slogan"];
        public string LoinPoweredBy => _configuration["LoinPage:PoweredBy"];
        public string LoinICP => _configuration["LoinPage:ICP"];
        public string LoinPolice => _configuration["LoinPage:Police"];
    }
}