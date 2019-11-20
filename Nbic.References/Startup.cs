﻿// ReSharper disable once StyleCop.SA1634
// ReSharper disable StyleCop.SA1600
namespace Nbic.References
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Data.Sqlite;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.OpenApi.Models;

    using Nbic.References.EFCore;
    using Nbic.References.Swagger;

    using Index = Nbic.Indexer.Index;

    public class Startup
    {
        private readonly string apiName;

        private readonly string authApiSecret;

        private readonly string authAuthority;

        private readonly string authAuthorityEndPoint;

        private readonly string connectionString;

        private readonly string provider;

        private ILogger logger;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            // configuration
            authAuthority = Configuration.GetValue("AuthAuthority", "https://demo.identityserver.io");
            authAuthorityEndPoint = Configuration.GetValue(
                "AuthAuthorityEndPoint",
                "https://demo.identityserver.io/connect/authorize");
            apiName = Configuration.GetValue("ApiName", "api");
            authApiSecret = Configuration.GetValue("AuthApiSecret", "test-secret");

            provider = Configuration.GetValue("DbProvider", "Sqlite");
            connectionString = Configuration.GetValue("DbConnectionString", "DataSource=:memory:");
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            this.logger = logger;
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                logger.LogInformation("In Development environment");
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            AddSwaggerMiddleware(app);

            app.UseCors("AllowAll");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            // services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            AddIdentityServerAuthentication(services);
            AddSwaggerGenerator(services);
            switch (provider)
            {
                case "Sqlite":
                    AddSqliteContext(services);
                    break;
                case "SqlServer":
                    AddSqlServerContext(services);
                    break;
            }

            services.AddSingleton(new Index());
            services.AddCors(
                options =>
                    {
                        options.AddPolicy(
                            "AllowAll",
                            builder =>
                                {
                                    builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()
                                        .WithExposedHeaders("WWW-Authenticate");
                                });
                    });
        }

        private void AddIdentityServerAuthentication(IServiceCollection services)
        {
            services.AddAuthorization();
            services.AddCors();

            services.AddAuthentication("token").AddIdentityServerAuthentication(
                "token",
                options =>
                    {
                        options.Authority = authAuthority;
                        options.RequireHttpsMetadata = false;

                        options.ApiName = apiName;
                        options.ApiSecret = authApiSecret;

                        options.JwtBearerEvents = new JwtBearerEvents
                                                      {
                                                          OnMessageReceived = e =>
                                                              {
                                                                  // _logger.LogTrace("JWT: message received");
                                                                  return Task.CompletedTask;
                                                              },
                                                          OnTokenValidated = e =>
                                                              {
                                                                  // _logger.LogTrace("JWT: token validated");
                                                                  return Task.CompletedTask;
                                                              },
                                                          OnAuthenticationFailed = e =>
                                                              {
                                                                  // _logger.LogTrace("JWT: authentication failed");
                                                                  return Task.CompletedTask;
                                                              },
                                                          OnChallenge = e =>
                                                              {
                                                                  // _logger.LogTrace("JWT: challenge");
                                                                  return Task.CompletedTask;
                                                              }
                                                      };
                    });
        }

        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
        [SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        private void AddSqliteContext(IServiceCollection services)
        {
            Console.WriteLine("Adding SqliteContext");
            if (connectionString == "DataSource=:memory:")
            {
                Console.WriteLine("SqliteContext - ConnectionString:" + connectionString);
                var context = new SqliteReferencesDbContext(connectionString);
                context.Database.OpenConnection();
                context.Database.Migrate();
                services.AddSingleton<ReferencesDbContext>(context); // in memory version need this
                Console.WriteLine("Added InMemoryDatabase");
            }
            else
            {
                // if database is empty - initiate
                if (!Directory.Exists("Data")) Directory.CreateDirectory("Data");
                var dbConnectionString = connectionString.Contains('/', StringComparison.InvariantCulture)
                                             ? connectionString
                                             : connectionString.Replace(
                                                 "Data Source=",
                                                 "Data Source=./Data/",
                                                 StringComparison.InvariantCulture);
                Console.WriteLine("SqliteContext - ConnectionString:" + dbConnectionString);
                using (var context = new SqliteReferencesDbContext(dbConnectionString))
                {
                    try
                    {
                        var any = context.Reference.Any();
                    }
                    catch (SqliteException ex)
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

                services.AddDbContext<ReferencesDbContext>(options => options.UseSqlite(dbConnectionString));
                Console.WriteLine("Added SqliteContext");
            }
        }

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        private void AddSqlServerContext(IServiceCollection services)
        {
            Console.WriteLine("Adding SqlServerContext");
            using (var context = new SqlServerReferencesDbContext(connectionString))
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

            services.AddDbContext<ReferencesDbContext>(options => options.UseSqlServer(connectionString));
            Console.WriteLine("Added SqlServerContext");
        }

        private void AddSwaggerGenerator(IServiceCollection services)
        {
            services.AddSwaggerGen(
                c =>
                    {
                        c.SwaggerDoc(
                            "v1",
                            new OpenApiInfo { Title = "Nbic References API via Swagger", Version = "v1" });

                        c.AddSecurityDefinition(
                            "oauth2",
                            new OpenApiSecurityScheme
                                {
                                    Type = SecuritySchemeType.OAuth2,
                                    Flows = new OpenApiOAuthFlows
                                                {
                                                    Implicit = new OpenApiOAuthFlow
                                                                   {
                                                                       AuthorizationUrl =
                                                                           new Uri(
                                                                               authAuthorityEndPoint,
                                                                               UriKind.Absolute),
                                                                       Scopes = new Dictionary<string, string>
                                                                                    {
                                                                                        { apiName, "Access Api" }

                                                                                        // { "readAccess", "Access read operations" },
                                                                                        // { "writeAccess", "Access write operations" }
                                                                                    }
                                                                   }
                                                }
                                });
                        c.OperationFilter<SecurityRequirementsOperationFilter>();
                        c.EnableAnnotations();
                    });
        }

        private void AddSwaggerMiddleware(IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(
                c =>
                    {
                        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Nbic References API V1");

                        c.OAuthClientId("implicit");
                        c.OAuthClientSecret(authApiSecret);
                        c.OAuthRealm("test-realm");
                        c.OAuthAppName(apiName);
                        c.OAuthScopeSeparator(" ");

                        // c.OAuthAdditionalQueryStringParams(new { foo = "bar" });
                        c.OAuthUseBasicAuthenticationWithAccessCodeGrant();
                        c.RoutePrefix = string.Empty;
                    });
        }
    }
}