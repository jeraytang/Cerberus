using System;
using Cerberus.Common;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace Cerberus
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
            services.AddHealthChecks();
            services.AddOptions();
            services.AddScoped<CerberusOptions>();

            services.AddControllersWithViews()
                .AddRazorRuntimeCompilation();

            services.AddHttpContextAccessor();

            services.AddRouting(x => x.LowercaseUrls = true);

            var options = new CerberusOptions(Configuration);

            services.AddResponseCompression();
            services.AddResponseCaching();

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

                    x.Scope.Add("openid");
                    x.Scope.Add("profile");
                    x.Scope.Add("role");
                    x.Scope.Add("cerberus-api");

                    x.ResponseType = "id_token token";

                    x.GetClaimsFromUserInfoEndpoint = true;

                    x.ClaimActions.MapAllExcept("iss", "nbf", "exp", "aud", "nonce", "iat", "c_hash");
                    x.ClaimActions.MapJsonKey("role", "role");
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = JwtClaimTypes.Name, RoleClaimType = JwtClaimTypes.Role,
                    };
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                Console.WriteLine($"EnvironmentName: {env.EnvironmentName}");
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
                endpoints.MapHealthChecks("/health");
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseResponseCompression();
            app.UseResponseCaching();
        }
    }
}