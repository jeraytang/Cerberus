using System;
using AutoMapper;
using IdentityServer4.AspNetIdentity;
using IdentityServer4.SecurityTokenService.Extensions;
using IdentityServer4.MySql;
using IdentityServer4.SecurityTokenService.Common;
using IdentityServer4.SecurityTokenService.Data;
using IdentityServer4.SecurityTokenService.Endpoints;
using IdentityServer4.SecurityTokenService.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using AutoMapperProfile = IdentityServer4.SecurityTokenService.Data.AutoMapperProfile;

namespace IdentityServer4.SecurityTokenService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<STSOptions>();

            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();

            var stsOptions = new STSOptions(Configuration);
            services.AddDbContextPool<IdentityDbContext>(builder =>
                builder.UseMySql(
                    string.IsNullOrWhiteSpace(stsOptions.IdentityConnectionString)
                        ? stsOptions.ConnectionString
                        : stsOptions.IdentityConnectionString));

            services.AddIdentity<IdentityUser, IdentityRole>(options =>
                {
                    var identityOptions = Configuration.GetIdentityOptions();
                    var config = new MapperConfiguration(cfg => { cfg.AddProfile<AutoMapperProfile>(); });
                    var mapper = new Mapper(config);
                    mapper.Map(identityOptions, options);
                })
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<IdentityDbContext>();

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = ".sts";
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                options.LoginPath = "/Account/Login";
                options.SlidingExpiration = true;
            });

            services.AddIdentityServer()
                .AddMySqlStoreCache()
                .AddProfileService<ProfileService<IdentityUser>>()
                .AddAspNetIdentity<IdentityUser>()
                .AddDeveloperSigningCredential();
            services.AddAntiforgery(x => { x.Cookie.Name = ".sts.antiforgery"; });
            services.AddAuthentication()
                .AddGoogle("Google", options =>
                {
                    options.SignInScheme = IdentityConstants.ExternalScheme;

                    options.ClientId = "<insert here>";
                    options.ClientSecret = "<insert here>";
                })
                .AddOpenIdConnect("oidc", "Demo IdentityServer", options =>
                {
                    options.SignInScheme = IdentityConstants.ExternalScheme;
                    options.SaveTokens = true;

                    options.Authority = "https://demo.identityserver.io/";
                    options.ClientId = "native.code";
                    options.ClientSecret = "secret";
                    options.ResponseType = "code";

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "name", RoleClaimType = "role"
                    };
                });
            services.AddControllersWithViews()
                .AddRazorRuntimeCompilation();
            services.AddRouting(options => options.LowercaseUrls = true);
            services.AddMemoryCache();
            services.AddScoped<IRedirectNotificationService, RedirectNotificationService>();

            services.AddTransient<TokenEndpoint>();
            services.AddSingleton(new Hosting.Endpoint("ExternalToken", "/connect/external-token",
                typeof(TokenEndpoint)));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // app.UseHttpsRedirection();

            app.UseCookiePolicy(new CookiePolicyOptions
            {
                MinimumSameSitePolicy = SameSiteMode.Lax
            });

            app.UseStaticFiles();

            app.UseRouting();

            app.UseMySqlIdentityServer();
            app.MigrateIdentity();

            app.UseAuthorization();
            app.UseAccessDenied();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}