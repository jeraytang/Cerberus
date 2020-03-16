using Microsoft.Extensions.Configuration;

namespace IdentityServer4.Storage.Adapter
{
    public class IdentityServerOptions
    {
        private readonly IConfiguration _configuration;


        public bool EnableAdmin => !string.IsNullOrWhiteSpace(_configuration["Admin:Enable"]) &&
                                   bool.Parse(_configuration["Admin:Enable"]);

        public string ClientId => string.IsNullOrWhiteSpace(_configuration["Admin:ClientId"])
            ? string.Empty
            : _configuration["Admin:ClientId"];

        public string AdminEmail => _configuration["Admin:Email"];

        public string AdminPassword => string.IsNullOrWhiteSpace(_configuration["Admin:Password"])
            ? "1qazZAQ!"
            : _configuration["Admin:Password"];

        public string RedirectUri => string.IsNullOrWhiteSpace(_configuration["Admin:RedirectUri"])
            ? string.Empty
            : _configuration["Admin:RedirectUri"];

        public string ClientSecret => string.IsNullOrWhiteSpace(_configuration["Admin:ClientSecret"])
            ? string.Empty
            : _configuration["Admin:ClientSecret"];

        public string PostLogoutRedirectUri =>
            string.IsNullOrWhiteSpace(_configuration["Admin:PostLogoutRedirectUri"])
                ? string.Empty
                : _configuration["Admin:PostLogoutRedirectUri"];

        public string ConnectionString => _configuration["ConnectionString"];

        /// <summary>
        /// Gets or sets a value indicating whether stale entries will be automatically cleaned up from the database.
        /// This is implemented by periodically connecting to the database (according to the TokenCleanupInterval) from the hosting application.
        /// Defaults to false.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable token cleanup]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableTokenCleanup =>
            !string.IsNullOrWhiteSpace(_configuration["IdentityServer4:EnableTokenCleanup"]) &&
            bool.Parse(_configuration["IdentityServer4:EnableTokenCleanup"]);

        /// <summary>
        /// Gets or sets the token cleanup interval (in seconds). The default is 3600 (1 hour).
        /// </summary>
        /// <value>
        /// The token cleanup interval.
        /// </value>
        public int TokenCleanupInterval =>
            string.IsNullOrWhiteSpace(_configuration["IdentityServer4:TokenCleanupInterval"])
                ? 3600
                : int.Parse(_configuration["IdentityServer4:TokenCleanupInterval"]);

        /// <summary>
        /// Gets or sets the number of records to remove at a time. Defaults to 100.
        /// </summary>
        /// <value>
        /// The size of the token cleanup batch.
        /// </value>
        public int TokenCleanupBatchSize =>
            string.IsNullOrWhiteSpace(_configuration["IdentityServer4:TokenCleanupBatchSize"])
                ? 100
                : int.Parse(_configuration["IdentityServer4:TokenCleanupBatchSize"]);

        public IdentityServerOptions(IConfiguration configuration)
        {
            _configuration = configuration;
        }
    }
}
