using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Nbic.References.Swagger;

/// <summary>
/// Help swagger discover api operations that require authentication
/// </summary>
public class SecurityRequirementsOperationFilter : IOperationFilter
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "<Pending>")]
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Policy names map to scopes
        var requiredScopes = context.MethodInfo
            .GetCustomAttributes(true)
            .OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>()
            .Select(attr => attr.Policy)
            .Distinct().ToList();

        if (requiredScopes.Count == 0) return;
            
        operation.Responses.Add("401", new() { Description = "Unauthorized" });
        operation.Responses.Add("403", new() { Description = "Forbidden" });

        var oAuthScheme = new OpenApiSecurityScheme
        {
            Reference = new() { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
        };

        operation.Security = new List<OpenApiSecurityRequirement>
        {
            new()
            {
                [ oAuthScheme ] = requiredScopes
            }
        };
    }
}