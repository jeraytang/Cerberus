using IdentityServer4.Storage.Adapter;
using IdentityServer4.Storage.Adapter.Repository;
using IdentityServer4.Storage.MySql.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityServer4.Storage.MySql
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddIdentityServer4Repository(this IServiceCollection services)
        {
            services.AddScoped<IdentityServerOptions>();
            services.AddScoped<IClientRepository, ClientRepository>();
            services.AddScoped<IApiResourceRepository, ApiResourceRepository>();
            services.AddScoped<IIdentityResourceRepository, IdentityResourceRepository>();
            return services;
        }
    }
}
