using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nbic.References.EFCore;
using Swashbuckle.AspNetCore.Swagger;

namespace Nbic.References
{
    public class Startup
    {
        private readonly string _authAuthority;
        private readonly string _authAuthorityEndPoint;
        private readonly string _authApiSecret;
        private readonly string _apiName;

        private readonly string _dbProvider;
        private readonly string _dbConnectionString;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            // config
            _authAuthority = Configuration.GetValue("AuthAuthority", "https://demo.identityserver.io");
            _authAuthorityEndPoint = Configuration.GetValue("AuthAuthorityEndPoint", "https://demo.identityserver.io/connect/authorize");
            _apiName = Configuration.GetValue("ApiName", "api");
            _authApiSecret = Configuration.GetValue("AuthApiSecret", "test-secret");

            _dbProvider = Configuration.GetValue("DbProvider", "Sqlite");
            _dbConnectionString = Configuration.GetValue("DbConnectionString", "DataSource=:memory:");
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            
            AddIdentityServerAuthentication(services);
            AddSwaggerGenerator(services);
            switch (_dbProvider)
            {
                case "Sqlite":
                    
                    AddSqliteContext(services);
                    break;
                case "SqlServer":
                    AddSqlServerContext(services);
                    break;
            }
           
        }

        private void AddSqlServerContext(IServiceCollection services)
        {
            using (var context = new SqlServerReferencesDbContext(_dbConnectionString))
            {
                try
                {
                    var any = context.RfReference.Any();
                }
                catch (Microsoft.Data.Sqlite.SqliteException ex)
                {
                    if (ex.Message.Contains("SQLite Error 1: 'no such table"))
                    {
                        context.Database.Migrate();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            services.AddDbContext<ReferencesDbContext>(options =>
                options.UseSqlServer(_dbConnectionString));
        }

        private void AddSqliteContext(IServiceCollection services)
        {
            if (_dbConnectionString == "DataSource=:memory:")
            {
                var context = new SqliteReferencesDbContext(_dbConnectionString);
                context.Database.OpenConnection();
                context.Database.Migrate();
                services.AddSingleton<ReferencesDbContext>(context); // in memory version need this
            }
            else
            {
                // if database is empty - initiate
                using (var context = new SqliteReferencesDbContext(_dbConnectionString))
                {
                    try
                    {
                        var any = context.RfReference.Any();
                    }
                    catch (Microsoft.Data.Sqlite.SqliteException ex)
                    {
                        if (ex.Message.Contains("SQLite Error 1: 'no such table"))
                        {
                            context.Database.Migrate();
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                services.AddDbContext<ReferencesDbContext>(options =>
                    options.UseSqlite(_dbConnectionString));
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            AddSwaggerMiddleware(app);
            app.UseCors(policy =>
            {
                policy.WithOrigins(
                    "http://localhost:28895",
                    "http://localhost:7017");

                policy.AllowAnyHeader();
                policy.AllowAnyMethod();
                policy.WithExposedHeaders("WWW-Authenticate");
            });

            app.UseAuthentication();
            app.UseMvc();
        }

        private void AddSwaggerGenerator(IServiceCollection services)
        {
// swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info {Title = "Nbic References API via Swagger", Version = "v1"});

                c.AddSecurityDefinition("oauth2", new OAuth2Scheme
                {
                    Type = "oauth2",
                    Flow = "implicit",
                    AuthorizationUrl = _authAuthorityEndPoint,
                    Scopes = new Dictionary<string, string>
                    {
                        {_apiName, "Access Api"}
                    }
                });
                c.OperationFilter<SecurityRequirementsOperationFilter>();
            });
            //services.AddSwaggerDocument();
        }

        private void AddIdentityServerAuthentication(IServiceCollection services)
        {
            services.AddAuthorization();
            services.AddCors();

            services.AddAuthentication("token")
                .AddIdentityServerAuthentication("token", options =>
                {
                    options.Authority = _authAuthority; // id.artsdatabanken.no
                    options.RequireHttpsMetadata = false;

                    options.ApiName = _apiName;
                    options.ApiSecret = _authApiSecret;

                    options.JwtBearerEvents = new JwtBearerEvents
                    {
                        OnMessageReceived = e =>
                        {
                            //_logger.LogTrace("JWT: message received");
                            return Task.CompletedTask;
                        },

                        OnTokenValidated = e =>
                        {
                            //_logger.LogTrace("JWT: token validated");
                            return Task.CompletedTask;
                        },

                        OnAuthenticationFailed = e =>
                        {
                            //_logger.LogTrace("JWT: authentication failed");
                            return Task.CompletedTask;
                        },

                        OnChallenge = e =>
                        {
                            //_logger.LogTrace("JWT: challenge");
                            return Task.CompletedTask;
                        }
                    };
                });
        }

        private void AddSwaggerMiddleware(IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Nbic References API V1");

                c.OAuthClientId("implicit");
                c.OAuthClientSecret(_authApiSecret);
                c.OAuthRealm("test-realm");
                c.OAuthAppName(_apiName);
                c.OAuthScopeSeparator(" ");
                //c.OAuthAdditionalQueryStringParams(new { foo = "bar" });
                c.OAuthUseBasicAuthenticationWithAccessCodeGrant();
            });
            //app.UseSwagger();
            //app.UseSwaggerUi3();
            //app.UseReDoc();
        }
    }
}
