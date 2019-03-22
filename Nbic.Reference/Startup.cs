using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;

namespace Nbic.Reference
{
    public class Startup
    {
        private readonly string _authAuthority;
        private readonly string _authAuthorityEndPoint;
        private readonly string _authApiSecret;
        private readonly string _apiName;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            // config
            _authAuthority = Configuration.GetValue("AuthAuthority", "https://demo.identityserver.io");
            _authAuthorityEndPoint = Configuration.GetValue("AuthAuthorityEndPoint", "https://demo.identityserver.io/connect/authorize");
            _apiName = Configuration.GetValue("ApiName", "api");
            _authApiSecret = Configuration.GetValue("AuthApiSecret", "test-secret");
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            
            AddIdentityServerAuthentication(services);
            AddSwaggerGenerator(services);
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Nbic Reference API V1");

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
