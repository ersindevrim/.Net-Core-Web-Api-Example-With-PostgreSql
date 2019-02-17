using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Swagger;
using Todo2Api.Helpers;
using Todo2Api.Services;
using Todo2Api.V1.Entities;

namespace Todo2Api {
    public class Startup {
        public Startup (IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices (IServiceCollection services) {
            services.AddCors ();

            services.AddDbContext<TodoContext> (options =>
                options.UseNpgsql (Configuration.GetConnectionString ("DefaultConnection")));

            services.AddSwaggerGen (x => {
                x.SwaggerDoc ("v1", new Info { Title = "Core Api", Description = "Swagger Core Api" });
                x.SwaggerDoc ("v2", new Info { Title = "Core Api", Description = "Swagger Core Api" });
                x.OperationFilter<SwaggerDefaultValues> ();
                // Swagger 2.+ support
                var security = new Dictionary<string, IEnumerable<string>> { { "Bearer", new string[] { } }, };

                x.AddSecurityDefinition ("Bearer", new ApiKeyScheme {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                        Name = "Authorization",
                        In = "header",
                        Type = "apiKey"
                });
                x.AddSecurityRequirement (security);
            });

            services.AddMvc ().SetCompatibilityVersion (CompatibilityVersion.Version_2_1);
            services.AddMvcCore ()
                .AddJsonFormatters ()
                .AddVersionedApiExplorer (
                    options => {
                        //The format of the version added to the route URL  
                        options.GroupNameFormat = "'v'VVV";
                        //Tells swagger to replace the version in the controller route  
                        options.SubstituteApiVersionInUrl = true;
                    }
                );

            services.AddApiVersioning (o => {
                o.ReportApiVersions = true;
                //o.AssumeDefaultVersionWhenUnspecified = true;
                //o.DefaultApiVersion = new ApiVersion (1, 0);
            });

            // configure strongly typed settings objects
            var appSettingsSection = Configuration.GetSection ("AppSettings");
            services.Configure<AppSettings> (appSettingsSection);

            // configure jwt authentication
            var appSettings = appSettingsSection.Get<AppSettings> ();
            var key = Encoding.ASCII.GetBytes (appSettings.Secret);
            services.AddAuthentication (x => {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer (x => {
                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey (key),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });

            // configure DI for application services
            services.AddScoped<IUserService, UserService> ();
        }

        public void Configure (IApplicationBuilder app, IHostingEnvironment env, IApiVersionDescriptionProvider provider) {
            if (env.IsDevelopment ()) {
                app.UseDeveloperExceptionPage ();
            } else {
                app.UseHsts ();
            }
            app.UseCors (x => x
                .AllowAnyOrigin ()
                .AllowAnyMethod ()
                .AllowAnyHeader ());

            app.UseAuthentication ();
            app.UseHttpsRedirection ();
            app.UseMvc ();
            app.UseSwagger ();
            app.UseSwaggerUI (x => {
                //x.SwaggerEndpoint("/swagger/v1/swagger.json","Core API");
                foreach (var description in provider.ApiVersionDescriptions) {
                    x.SwaggerEndpoint ($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant ());
                }
            });
        }
    }
}