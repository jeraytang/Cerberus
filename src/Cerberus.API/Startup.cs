using AutoMapper;
using Cerberus.API.Common;
using Cerberus.API.Data;
using Cerberus.API.DTO;
using Cerberus.API.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MSFramework;
using MSFramework.AspNetCore;
using MSFramework.AutoMapper;

namespace Cerberus.API
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
			services.AddControllers(x =>
			{
				x.Filters.Add<HttpGlobalExceptionFilter>();
			}).ConfigureApiBehaviorOptions(x =>
			{
				x.InvalidModelStateResponseFactory = InvalidModelStateResponseFactory.Instance;
			}).AddNewtonsoftJson();

			services.AddRouting(x => x.LowercaseUrls = true);

			services.AddResponseCompression();
			services.AddResponseCaching();

			var options = new CerberusOptions(Configuration);
			services.AddDbContextPool<IdentityDbContext>(builder =>
			{
				builder.UseMySql(
					options.ConnectionString);
			});

			services.AddIdentityCore<User, Role>(x =>
				{
					var identityOptions = Configuration.GetIdentityOptions();
					var config = new MapperConfiguration(cfg => { cfg.AddProfile<AutoMapperProfile>(); });
					var mapper = new Mapper(config);
					mapper.Map(identityOptions, x);
					x.User.RequireUniqueEmail = true;
				})
				.AddEntityFrameworkStores<IdentityDbContext>();

			services.AddMemoryCache();

			services.AddMSFramework(x =>
			{
				x.AddEventBus(typeof(Startup));
				x.AddAspNetCore();
				x.AddAutoMapper(typeof(AutoMapperProfile));
			});

			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1.0", new OpenApiInfo {Version = "v1.0", Description = "Cerberus API V1.0"});
			});
			
			services.AddAuthentication("Bearer")
				.AddIdentityServerAuthentication(x =>
				{
					x.Authority = options.Authority;
					x.RequireHttpsMetadata = options.RequireHttpsMetadata;
					x.ApiName = options.Audience;
					if (!string.IsNullOrWhiteSpace(options.ApiSecret))
					{
						x.ApiSecret = options.ApiSecret;
					}
				});

			services.AddCors();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.UseMSFramework();
			app.Migrate();

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			// app.UseHttpsRedirection();

			app.UseRouting();

			app.UseCors(x =>
			{
				x.AllowAnyHeader();
				x.AllowAnyOrigin();
				x.AllowAnyMethod();
			});

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapHealthChecks("/health");
				endpoints.MapControllers();
			});

			app.UseResponseCompression();
			app.UseResponseCaching();

			var exit = Configuration["exit"] == "true";
			if (exit)
			{
				app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>().StopApplication();
			}

			//启用中间件服务生成Swagger作为JSON终结点
			app.UseSwagger();
			//启用中间件服务对swagger-ui，指定Swagger JSON终结点
			app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "Cerberus API V1.0"); });
		}
	}
}
