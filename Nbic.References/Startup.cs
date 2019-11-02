using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nbic.References.EFCore;
using Microsoft.OpenApi.Models;
using Nbic.References.Swagger;

namespace Nbic.References
{
    public class Startup
    {
        private ILogger _logger;

        private readonly string _authAuthority;
        private readonly string _authAuthorityEndPoint;
        private readonly string _authApiSecret;
        private readonly string _apiName;

        private readonly string _dbProvider;
        private readonly string _dbConnectionString;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            // configuration
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
            services.AddControllers();
            //services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
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

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            _logger = logger;
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                logger.LogInformation("In Development environment");
            }

            app.UseHttpsRedirection();

            app.UseRouting();

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
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        private void AddSqlServerContext(IServiceCollection services)
        {
            Console.WriteLine("Adding SqlServerContext");
            using (var context = new SqlServerReferencesDbContext(_dbConnectionString))
            {
                try
                {
                    var any = context.Reference.Any();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Empty Schema - initial create - after error " + ex.Message);
                    context.Database.Migrate();
                }
            }
            services.AddDbContext<ReferencesDbContext>(options =>
                options.UseSqlServer(_dbConnectionString));
            Console.WriteLine("Added SqlServerContext");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
        private void AddSqliteContext(IServiceCollection services)
        {
            Console.WriteLine("Adding SqliteContext");
            if (_dbConnectionString == "DataSource=:memory:")
            {
                Console.WriteLine("SqliteContext - ConnectionString:" + _dbConnectionString);
                var context = new SqliteReferencesDbContext(_dbConnectionString);
                context.Database.OpenConnection();
                context.Database.Migrate();
                services.AddSingleton<ReferencesDbContext>(context); // in memory version need this
                Console.WriteLine("Added InMemoryDatabase");
            }
            else
            {
                // if database is empty - initiate

                if (!System.IO.Directory.Exists("Data")) System.IO.Directory.CreateDirectory("Data");
                var dbConnectionString = _dbConnectionString.Contains('/', StringComparison.InvariantCulture) ? _dbConnectionString : _dbConnectionString.Replace("Data Source=", "Data Source=./Data/", StringComparison.InvariantCulture);
                Console.WriteLine("SqliteContext - ConnectionString:" + dbConnectionString);
                using (var context = new SqliteReferencesDbContext(dbConnectionString))
                {
                    try
                    {
                        var any = context.Reference.Any();
                    }
                    catch (Microsoft.Data.Sqlite.SqliteException ex)
                    {
                        if (ex.Message.Contains("SQLite Error 1: 'no such table", StringComparison.CurrentCulture))
                        {
                            context.Database.Migrate();
                            Console.WriteLine("Empty Schema - initial create - after exception " + ex.Message);
                        }
                        else
                        {
                            throw;
                        }
                    }
                    Console.WriteLine("Added FileDatabase");
                }

                services.AddDbContext<ReferencesDbContext>(options =>
                    options.UseSqlite(dbConnectionString));
                Console.WriteLine("Added SqliteContext");
            }
        }

        private void AddSwaggerGenerator(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(  "v1", new OpenApiInfo() { Title = "Nbic References API via Swagger", Version = "v1"});

                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        Implicit = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri(_authAuthorityEndPoint, UriKind.Absolute),
                            Scopes = new Dictionary<string, string>
                            {
                                {_apiName, "Access Api"}
                                //{ "readAccess", "Access read operations" },
                                //{ "writeAccess", "Access write operations" }
                            }
                        }
                    }
                });
                c.OperationFilter<SecurityRequirementsOperationFilter>();
                c.EnableAnnotations();
            });
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
                c.RoutePrefix = "";
            });
        }
    }
}
