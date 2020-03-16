using AutoMapper;
using IdentityServer4.Admin.Common;
using IdentityServer4.Storage.Adapter;
using IdentityServer4.Storage.MySql;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AutoMapperProfile = IdentityServer4.Admin.DTO.AutoMapperProfile;

namespace IdentityServer4.Admin
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
            services.AddScoped<AdminOptions>();
            services.AddScoped<IdentityServerOptions>();

            services.AddControllersWithViews()
                .AddRazorRuntimeCompilation()
                .ConfigureApiBehaviorOptions(x =>
                {
                    x.InvalidModelStateResponseFactory = InvalidModelStateResponseFactory.Instance;
                }).AddNewtonsoftJson();

            services.AddIdentityServer4Repository();

            var options = new AdminOptions(Configuration);
            services.AddAuthentication(x =>
                {
                    x.DefaultScheme = "Cookies";
                    x.DefaultChallengeScheme = "oidc";
                })
                .AddCookie("Cookies")
                .AddOpenIdConnect("oidc", x =>
                {
                    x.SignInScheme = "Cookies";
                    x.Authority = options.Authority;
                    x.RequireHttpsMetadata = options.RequireHttpsMetadata;
                    x.ClientId = options.ClientId;
                    x.ClientSecret = options.ClientSecret;
                    x.SaveTokens = true;
                    x.ClientSecret = options.Secret;
                    x.GetClaimsFromUserInfoEndpoint = true;
                    x.CallbackPath = new PathString("/signin-oidc");

                    x.Scope.Add("openid");
                    x.Scope.Add("profile");
                    x.Scope.Add("role");
                });

            services.AddRouting(x => x.LowercaseUrls = true);
            services.AddAutoMapper(typeof(AutoMapperProfile));
            services.AddSingleton<IActionResultTypeMapper, ActionResultTypeMapper>();
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

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}