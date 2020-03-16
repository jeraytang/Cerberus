using AutoMapper;
using IdentityServer4.MySql.Extensions;
using IdentityServer4.MySql.Services;
using IdentityServer4.Storage.MySql;
using IdentityServer4.Storage.MySql.Stores;
using IdentityServer4.Storage.Adapter;
using IdentityServer4.Storage.MySql.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IdentityServer4.MySql
{
	public static class ServiceCollectionExtensions
	{
		public static IApplicationBuilder UseMySqlIdentityServer(
			this IApplicationBuilder app,
			IdentityServerMiddlewareOptions options = null)
		{
			app.UseIdentityServer(options);

			using var scope = app.ApplicationServices.CreateScope();
			var mySqlOptions = scope.ServiceProvider.GetRequiredService<IdentityServerOptions>();
			mySqlOptions.Migrate();
			app.Init();

			return app;
		}

		public static IIdentityServerBuilder AddMySqlStore(this IIdentityServerBuilder builder)
		{
			builder.AddCore();

			builder.AddClientStore<ClientStore>()
				.AddResourceStore<ResourceStore>()
				.AddDeviceFlowStore<DeviceFlowStore>()
				.AddPersistedGrantStore<PersistedGrantStore>()
				.AddCorsPolicyService<CorsPolicyService>();

			return builder;
		}

		public static IIdentityServerBuilder AddMySqlStoreCache(this IIdentityServerBuilder builder)
		{
			builder.AddCore();

			builder.AddInMemoryCaching();

			// add the caching decorators
			builder.AddClientStoreCache<ClientStore>()
				.AddResourceStoreCache<ResourceStore>()
				.AddCorsPolicyCache<CorsPolicyService>()
				.AddDeviceFlowStore<DeviceFlowStore>()
				.AddPersistedGrantStore<PersistedGrantStore>();

			return builder;
		}

		private static IIdentityServerBuilder AddCore(this IIdentityServerBuilder builder)
		{
			builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
			builder.Services.AddIdentityServer4Repository();
			builder.Services.AddSingleton<IHostedService, TokenCleanupHost>();

			return builder;
		}
	}
}
