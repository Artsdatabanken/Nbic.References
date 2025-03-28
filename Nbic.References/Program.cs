namespace Nbic.References;

using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;

using Nbic.References.Infrastructure.Repositories;
using Nbic.References.Infrastructure.Repositories.DbContext;
using Nbic.References.Middleware;

using RobotsTxt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Nbic.References.Swagger;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;

using Index = Nbic.References.Infrastructure.Services.Indexing.Index;
using Microsoft.Extensions.Hosting;

using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

public class Program
{
    private static string apiName;
    private static string authAuthority;
    private static string authAuthorityEndPoint;
    private static string connectionString;
    private static string provider;
    private static string writeAccessRole;
    private static string swaggerClientId;

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // configuration
        GetConfiguration(builder);

        // Add services to the container.
        builder.Services.AddApplicationInsightsTelemetry();
        builder.Services.AddSingleton<ITelemetryInitializer, FilterHealthchecksTelemetryInitializer>();
        builder.Services.AddResponseCompression();
        builder.Services.AddControllers(options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);

        // builder.Services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
        AddIdentityServerAuthentication(builder.Services);
        AddSwaggerGenerator(builder.Services);
        switch (provider)
        {
            case "Sqlite":
                AddSqliteContext(builder.Services);
                break;
            case "SqlServer":
                AddSqlServerContext(builder.Services);
                break;
        }

        builder.Services.AddSingleton(new Index());
        builder.Services.AddScoped<IReferencesRepository, ReferenceRepository>();
        builder.Services.AddScoped<IReferenceUsageRepository, ReferenceUsageRepository>();

        builder.Services.AddCors(
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

        // health monitoring
        builder.Services.AddHealthChecks()
            .AddDbContextCheck<ReferencesDbContext>();

        // no search engine indexing
        builder.Services.AddStaticRobotsTxt(builder => builder.DenyAll());

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        app.UseResponseCompression();
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.Logger.LogInformation("In Development environment");

            IdentityModelEventSource.ShowPII = true;
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRobotsTxt();
        app.UseRouting();

        AddSwaggerMiddleware(app);

        app.UseCors("AllowAll");

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseHealthChecks("/hc");
        app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        app.Run();
    }

    private static void GetConfiguration(WebApplicationBuilder builder)
    {
        authAuthority = builder.Configuration.GetValue("AuthAuthority", "https://demo.duendesoftware.com/");
        authAuthorityEndPoint = builder.Configuration.GetValue(
            "AuthAuthorityEndPoint",
            "https://demo.duendesoftware.com/connect/authorize");
        apiName = builder.Configuration.GetValue("ApiName", "api");

        provider = builder.Configuration.GetValue("DbProvider", "Sqlite");
        connectionString = builder.Configuration.GetValue("DbConnectionString", "DataSource=:memory:");

        writeAccessRole = builder.Configuration.GetValue("WriteAccessRole", "my_write_access_role");
        swaggerClientId = builder.Configuration.GetValue("SwaggerClientId", "implicit");
    }

    private static void AddIdentityServerAuthentication(IServiceCollection services)
    {
        var roleClaimValue = writeAccessRole;
        
        // Users defined at https://demo.identityserver.com has no roles.
        // Using the Issuer-claim (iss) as a substitute to allow authorization with Swagger when testing
        if (authAuthority == "https://demo.identityserver.com")
        {
            roleClaimValue = "https://demo.identityserver.com";
        }

        services.AddAuthorization(options =>
            options.AddPolicy("WriteAccess", policy =>
            {
                policy.RequireRole(roleClaimValue);
                policy.RequireAuthenticatedUser();
                //policy.RequireClaim(roleClaim, roleClaimValue);
            }));

        services.AddCors();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = authAuthority;
            options.Audience = apiName;
            options.RequireHttpsMetadata = false;
            // audience is optional, make sure you read the following paragraphs
            // to understand your options
            options.TokenValidationParameters.ValidateAudience = false;
            // it's recommended to check the type header to avoid "JWT confusion" attacks
            options.TokenValidationParameters.ValidTypes = ["at+jwt"];
            //options.TokenValidationParameters = new TokenValidationParameters
            //{
            //    ValidateIssuer = true,
            //    ValidateAudience = true,
            //    ValidateLifetime = true,
            //    ValidateIssuerSigningKey = true,
            //    ValidIssuer = authAuthority,
            //    ValidAudience = apiName,
            //    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your_secret_key"))
            //};

            options.Events = new JwtBearerEvents
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
                    Console.WriteLine("JWT: authentication failed: " + e.Exception);
                    return Task.CompletedTask;
                },
                OnChallenge = e =>
                {
                    // _logger.LogTrace("JWT: challenge");
                    return Task.CompletedTask;
                }
            };
        });


        IdentityModelEventSource.ShowPII = true;
    }

    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
    [SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
    private static void AddSqliteContext(IServiceCollection services)
    {
        Console.WriteLine("Adding SqliteContext");
        if (connectionString == "DataSource=:memory:")
        {
            Console.WriteLine($"SqliteContext - ConnectionString:{connectionString}");
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
            Console.WriteLine($"SqliteContext - ConnectionString:{dbConnectionString}");
            using (var context = new SqliteReferencesDbContext(dbConnectionString))
            {
                try
                {
                    // ReSharper disable once UnusedVariable
                    var any = context.Reference.Any();
                }
                catch (SqliteException ex)
                {
                    if (ex.Message.Contains("SQLite Error 1: 'no such table", StringComparison.CurrentCulture))
                    {
                        context.Database.Migrate();
                        Console.WriteLine($"Empty Schema - initial create - after exception {ex.Message}");
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
    [SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
    private static void AddSqlServerContext(IServiceCollection services)
    {
        Console.WriteLine("Adding SqlServerContext");
        using (var context = new SqlServerReferencesDbContext(connectionString))
        {
            try
            {
                // ReSharper disable once UnusedVariable
                var any = context.Reference.Any();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Empty Schema - initial create - after error {ex.Message}");
                context.Database.Migrate();
            }
        }

        services.AddDbContext<ReferencesDbContext>(options => options.UseSqlServer(connectionString));
        Console.WriteLine("Added SqlServerContext");
    }

    private static void AddSwaggerGenerator(IServiceCollection services)
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
                                }
                            }
                        }
                    });
                c.OperationFilter<SecurityRequirementsOperationFilter>();
                c.EnableAnnotations();
            });
    }

    private static void AddSwaggerMiddleware(IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(
            c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Nbic References API V1");
                c.OAuthClientId(swaggerClientId);
                c.OAuthAppName(apiName);
                c.OAuthScopeSeparator(" ");

                // c.OAuthAdditionalQueryStringParams(new { foo = "bar" });
                c.OAuthUseBasicAuthenticationWithAccessCodeGrant();
                c.RoutePrefix = string.Empty;
                c.DocumentTitle = "Nbic Reference API - swagger documentation"; // Since swagger doc i index.html - modify title
            });
    }
}