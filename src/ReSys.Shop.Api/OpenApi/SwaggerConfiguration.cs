using Microsoft.Extensions.Options;
using ReSys.Shop.Infrastructure.Security.Authentication.Externals.Options;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

using FluentStorage.Utils.Extensions;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

using Serilog;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace ReSys.Shop.Api.OpenApi;

public static class SwaggerConfiguration
{
    internal static IServiceCollection AddSwaggerWithAuth(this IServiceCollection services)
    {
        services.AddSwaggerGen(setupAction: o =>
        {
            o.SwaggerDoc(name: "v1",
                info: new OpenApiInfo
                {
                    Title = "Stellar FashionShop API",
                    Version = "v1",
                    Description = "An API for managing fashion shop operations."
                });

            o.CustomSchemaIds(schemaIdSelector: type => SchemaIdStrategy.GenerateSchemaId(type: type));
            o.SchemaFilter<SnakeCaseSchemaFilter>();
            o.ParameterFilter<SnakeCaseParameterFilter>();
            o.OperationFilter<MultipartFormDataOperationFilter>();

            // Add security schemes
            o.AddSecurityDefinition(name: JwtBearerDefaults.AuthenticationScheme,
                securityScheme: CreateJwtSecurityScheme());
            o.AddSecurityDefinition(name: "Google",
                securityScheme: CreateGoogleOAuth2Scheme());
            o.AddSecurityDefinition(name: "Facebook",
                securityScheme: CreateFacebookOAuth2Scheme());

            // Add security requirements
            o.UseAllOfToExtendReferenceSchemas();
            o.AddSecurityRequirement(securityRequirement: CreateJwtSecurityRequirement());
            o.AddSecurityRequirement(securityRequirement: CreateGoogleSecurityRequirement());
            o.AddSecurityRequirement(securityRequirement: CreateFacebookSecurityRequirement());

            o.DocumentFilter<AuthEndpointOrderDocumentFilter>();
        });

        Log.Information(
            messageTemplate: "Register: Swagger with JWT, Google OAuth2, and Facebook OAuth2 authentication configuration.");
        return services;
    }

    internal static IApplicationBuilder UseSwaggerWithUi(this WebApplication app)
    {
        GoogleOption googleOptions = app.Services.GetRequiredService<IOptions<GoogleOption>>().Value;
        FacebookOption facebookOptions = app.Services.GetRequiredService<IOptions<FacebookOption>>().Value;

        app.UseSwagger(setupAction: options => options.RouteTemplate = "/openapi/{documentName}.json");
        app.UseSwaggerUI(setupAction: c =>
        {
            c.SwaggerEndpoint(url: "/openapi/v1.json",
                name: "Stellar FashionShop API V1");
            c.RoutePrefix = string.Empty;

            // OAuth2 configuration for Google (uses PKCE)
            c.OAuthClientId(value: googleOptions.ClientId);
            c.OAuthAppName(value: "Stellar FashionShop API");
            c.OAuthUsePkce();
            c.OAuthScopeSeparator(value: " ");

            // OAuth2 configuration for Facebook
            c.OAuthAdditionalQueryStringParams(value: new Dictionary<string, string>
            {
                { "response_type", "code" },
                { "client_id", facebookOptions.AppId }
            });

            // Set OAuth2 redirect URL (adjust based on your configuration)
            c.OAuth2RedirectUrl(url: $"{app.Configuration[key: "BaseUrl"] ?? "https://localhost"}/swagger/oauth2-redirect.html");

            // For Facebook, if you need client secret (not recommended for public clients)
            // c.OAuthClientSecret(facebookOptions.ClientSecret);
        });

        Log.Information(messageTemplate: "Use: Swagger with UI and OAuth2 configuration.");
        return app;
    }

    private static OpenApiSecurityScheme CreateJwtSecurityScheme()
    {
        return new OpenApiSecurityScheme
        {
            Name = "JWT Authentication",
            Description = "Enter your JWT token in this field",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = JwtBearerDefaults.AuthenticationScheme,
            BearerFormat = "JWT"
        };
    }

    private static OpenApiSecurityScheme CreateGoogleOAuth2Scheme()
    {
        return new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Description = "Google OAuth2 Authentication",
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri(uriString: "https://accounts.google.com/o/oauth2/v2/auth"),
                    TokenUrl = new Uri(uriString: "https://oauth2.googleapis.com/token"),
                    Scopes = new Dictionary<string, string>
                    {
                        { "openid", "OpenID Connect" },
                        { "profile", "User profile information" },
                        { "email", "User email address" }
                    }
                }
            }
        };
    }

    private static OpenApiSecurityScheme CreateFacebookOAuth2Scheme()
    {
        return new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Description = "Facebook OAuth2 Authentication",
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri(uriString: "https://www.facebook.com/v18.0/dialog/oauth"),
                    TokenUrl = new Uri(uriString: "https://graph.facebook.com/v18.0/oauth/access_token"),
                    Scopes = new Dictionary<string, string>
                    {
                        { "openid", "OpenID Connect" },
                        { "email", "User email address" },
                        { "public_profile", "User public profile information" }
                    }
                }
            }
        };
    }

    private static OpenApiSecurityRequirement CreateJwtSecurityRequirement()
    {
        return new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme, Id = JwtBearerDefaults.AuthenticationScheme
                    }
                },
                new List<string>()
            }
        };
    }

    private static OpenApiSecurityRequirement CreateGoogleSecurityRequirement()
    {
        return new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Google" }
                },
                new List<string> { "openid", "profile", "email" }
            }
        };
    }

    private static OpenApiSecurityRequirement CreateFacebookSecurityRequirement()
    {
        return new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Facebook" }
                },
                new List<string> { "email", "public_profile" }
            }
        };
    }

    public sealed class AuthEndpointOrderDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            // Define the desired order of tags, with "Authentication Management" and "Account" first
            var tagOrder = new List<string> { "Authentication", "Account" };

            // Reorder tags in the document
            var orderedTags = swaggerDoc.Tags
                ?.OrderBy(keySelector: tag =>
                {
                    int index = tagOrder.IndexOf(item: tag.Name);
                    return index == -1 ? int.MaxValue : index; // Place unlisted tags at the end
                })
                .ThenBy(keySelector: tag =>
                    tag.Name) // Alphabetical sort for tags with the same priority or unlisted tags
                .ToList() ?? [];

            swaggerDoc.Tags.AddRange(source: orderedTags);
        }
    }

    public sealed class SnakeCaseSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema.Properties == null) return;

            List<KeyValuePair<string, OpenApiSchema>> propertiesToUpdate = schema.Properties.ToList();
            schema.Properties.Clear();

            foreach ((string key, OpenApiSchema value) in propertiesToUpdate)
            {
                string snakeCaseKey = JsonNamingPolicy.SnakeCaseLower.ConvertName(name: key);
                schema.Properties[key: snakeCaseKey] = value;
            }
        }
    }

    public sealed class SnakeCaseParameterFilter : IParameterFilter
    {
        public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
        {
            if (parameter.In == ParameterLocation.Query)
            {
                parameter.Name = JsonNamingPolicy.SnakeCaseLower.ConvertName(name: parameter.Name);
            }
        }
    }

    public sealed class MultipartFormDataOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (!ConsumesMultipartFormData(operation: operation))
            return;

        var formParameters = GetFormParameters(context: context);
        if (!formParameters.Any())
            return;

        if (!HasFileUploads(formParameters: formParameters))
            return;

        ConfigureMultipartFormData(operation: operation, formParameters: formParameters, context: context);
    }

    private bool ConsumesMultipartFormData(OpenApiOperation operation)
    {
        return operation.RequestBody?.Content?.ContainsKey(key: "multipart/form-data") ?? false;
    }

    private List<ParameterInfo> GetFormParameters(OperationFilterContext context)
    {
        return context.MethodInfo.GetParameters()
            .Where(predicate: p => p.GetCustomAttribute<Microsoft.AspNetCore.Mvc.FromFormAttribute>() != null)
            .ToList();
    }

    private bool HasFileUploads(List<ParameterInfo> formParameters)
    {
        return formParameters.Any(predicate: p => ContainsFileUpload(type: p.ParameterType));
    }

    private void ConfigureMultipartFormData(
        OpenApiOperation operation,
        List<ParameterInfo> formParameters,
        OperationFilterContext context)
    {
        var mediaType = operation.RequestBody.Content[key: "multipart/form-data"];

        mediaType.Schema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>(),
            Required = new HashSet<string>()
        };

        operation.Parameters = operation.Parameters
            .Where(predicate: p => p.In == ParameterLocation.Path)
            .ToList();

        foreach (var param in formParameters)
        {
            ProcessFormParameter(parameter: param, schema: mediaType.Schema, context: context);
        }

        if (!mediaType.Schema.Properties.Any())
        {
            operation.RequestBody = null;
        }
    }

    private void ProcessFormParameter(
        ParameterInfo parameter,
        OpenApiSchema schema,
        OperationFilterContext context)
    {
        var paramType = parameter.ParameterType;
        var paramName = JsonNamingPolicy.SnakeCaseLower.ConvertName(name: parameter.Name ?? "parameter");

        // Direct IFormFile
        if (paramType == typeof(IFormFile))
        {
            schema.Properties[key: paramName] = CreateFileSchema();
            if (IsParameterRequired(parameter: parameter))
                schema.Required.Add(item: paramName);
            return;
        }

        // List<IFormFile>
        if (IsListOfFiles(type: paramType))
        {
            schema.Properties[key: paramName] = CreateFileArraySchema();
            return;
        }

        // Complex types
        ProcessComplexType(type: paramType, schema: schema, context: context);
    }

    private void ProcessComplexType(Type type, OpenApiSchema schema, OperationFilterContext context)
    {
        var properties = type.GetProperties(bindingAttr: BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            var propertyName = GetPropertyName(property: property);

            if (schema.Properties.ContainsKey(key: propertyName))
                continue;

            var propertySchema = CreatePropertySchema(property: property, context: context);
            if (propertySchema != null)
            {
                schema.Properties[key: propertyName] = propertySchema;

                if (IsPropertyRequired(property: property))
                {
                    schema.Required.Add(item: propertyName);
                }
            }
        }
    }

    private OpenApiSchema? CreatePropertySchema(PropertyInfo property, OperationFilterContext context)
    {
        var propType = property.PropertyType;

        // IFormFile
        if (propType == typeof(IFormFile))
        {
            return CreateFileSchema(description: property.Name);
        }

        // List<IFormFile>
        if (IsListOfFiles(type: propType))
        {
            return CreateFileArraySchema(description: property.Name);
        }

        // List<ComplexType> - KEY FIX: Handle nested lists with file uploads
        if (IsGenericList(type: propType))
        {
            var itemType = propType.GetGenericArguments()[0];
            
            // Check if list items contain file uploads
            if (ContainsFileUpload(type: itemType))
            {
                return new OpenApiSchema
                {
                    Type = "array",
                    Items = CreateSchemaForComplexType(type: itemType, context: context),
                    Description = $"Array of {itemType.Name}"
                };
            }
            
            // Simple type array
            return new OpenApiSchema
            {
                Type = "array",
                Items = CreateSimpleTypeSchema(type: itemType, context: context) ?? new OpenApiSchema { Type = "string" },
                Description = $"Array of {itemType.Name}"
            };
        }

        // Nested complex types
        if (IsComplexType(type: propType))
        {
            return CreateSchemaForComplexType(type: propType, context: context);
        }

        // Simple types
        return CreateSimpleTypeSchema(type: propType, context: context);
    }

    private OpenApiSchema CreateSchemaForComplexType(Type type, OperationFilterContext context)
    {
        var schema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>(),
            Required = new HashSet<string>()
        };

        var properties = type.GetProperties(bindingAttr: BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            var propertyName = GetPropertyName(property: property);
            var propertySchema = CreatePropertySchema(property: property, context: context);

            if (propertySchema != null)
            {
                schema.Properties[key: propertyName] = propertySchema;

                if (IsPropertyRequired(property: property))
                {
                    schema.Required.Add(item: propertyName);
                }
            }
        }

        return schema;
    }

    private OpenApiSchema? CreateSimpleTypeSchema(Type type, OperationFilterContext context)
    {
        try
        {
            return context.SchemaGenerator.GenerateSchema(modelType: type, schemaRepository: context.SchemaRepository);
        }
        catch
        {
            return new OpenApiSchema { Type = "string" };
        }
    }

    private OpenApiSchema CreateFileSchema(string? description = null)
    {
        return new OpenApiSchema
        {
            Type = "string",
            Format = "binary",
            Description = description != null ? $"File upload for {description}" : "File upload"
        };
    }

    private OpenApiSchema CreateFileArraySchema(string? description = null)
    {
        return new OpenApiSchema
        {
            Type = "array",
            Items = new OpenApiSchema { Type = "string", Format = "binary" },
            Description = description != null
                ? $"Multiple file uploads for {description}"
                : "Multiple file uploads"
        };
    }

    private string GetPropertyName(PropertyInfo property)
    {
        var fromFormAttr = property.GetCustomAttribute<Microsoft.AspNetCore.Mvc.FromFormAttribute>();
        var name = fromFormAttr?.Name ?? property.Name;
        
        // Apply snake_case conversion
        return JsonNamingPolicy.SnakeCaseLower.ConvertName(name: name);
    }

    private bool IsParameterRequired(ParameterInfo parameter)
    {
        if (parameter.GetCustomAttribute<RequiredAttribute>() != null)
            return true;

        if (parameter.ParameterType.IsValueType && 
            Nullable.GetUnderlyingType(nullableType: parameter.ParameterType) == null)
            return true;

        return false;
    }

    private bool IsPropertyRequired(PropertyInfo property)
    {
        if (property.GetCustomAttribute<RequiredAttribute>() != null)
            return true;

        if (property.PropertyType.IsValueType && 
            Nullable.GetUnderlyingType(nullableType: property.PropertyType) == null)
            return true;

        try
        {
            var nullabilityContext = new NullabilityInfoContext();
            var nullabilityInfo = nullabilityContext.Create(propertyInfo: property);
            return nullabilityInfo.WriteState == NullabilityState.NotNull;
        }
        catch
        {
            return false;
        }
    }

    private bool ContainsFileUpload(Type type)
    {
        if (type == typeof(IFormFile))
            return true;

        if (IsListOfFiles(type: type))
            return true;

        if (type.IsGenericType)
        {
            var genericArgs = type.GetGenericArguments();
            if (genericArgs.Any(predicate: ContainsFileUpload))
                return true;
        }

        if (!IsComplexType(type: type))
            return false;

        var properties = type.GetProperties(bindingAttr: BindingFlags.Public | BindingFlags.Instance);
        return properties.Any(predicate: prop => ContainsFileUpload(type: prop.PropertyType));
    }

    private bool IsComplexType(Type type)
    {
        if (type.IsPrimitive || type.IsEnum)
            return false;

        if (type == typeof(string) ||
            type == typeof(decimal) ||
            type == typeof(DateTime) ||
            type == typeof(DateTimeOffset) ||
            type == typeof(TimeSpan) ||
            type == typeof(Guid) ||
            type == typeof(byte[]))
            return false;

        var underlyingType = Nullable.GetUnderlyingType(nullableType: type);
        if (underlyingType != null)
            return IsComplexType(type: underlyingType);

        return true;
    }

    private bool IsGenericList(Type type)
    {
        if (!type.IsGenericType)
            return false;

        var genericDef = type.GetGenericTypeDefinition();
        return genericDef == typeof(List<>) ||
               genericDef == typeof(IList<>) ||
               genericDef == typeof(IEnumerable<>) ||
               genericDef == typeof(ICollection<>);
    }

    private bool IsListOfFiles(Type type)
    {
        if (!IsGenericList(type: type))
            return false;

        var itemType = type.GetGenericArguments()[0];
        return itemType == typeof(IFormFile);
    }
}
    private static class StringExtensions
    {
        public static string ToLowerCamelCase(string? str)
        {
            if (string.IsNullOrEmpty(value: str) || !char.IsUpper(c: str[index: 0]))
                return string.Empty;

            var chars = str.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                // Stop if we hit a lowercase letter after the first character
                if (i == 1 && !char.IsUpper(c: chars[i]))
                    break;

                bool hasNext = (i + 1 < chars.Length);

                // Handle acronyms: HTTPServer -> httpServer
                if (i > 0 && hasNext && !char.IsUpper(c: chars[i + 1]))
                {
                    if (char.IsLower(c: chars[i]))
                        break;

                    chars[i] = char.ToLowerInvariant(c: chars[i]);
                    break;
                }

                chars[i] = char.ToLowerInvariant(c: chars[i]);
            }

            return new string(value: chars);
        }
    }
}

public static class SchemaIdStrategy
{
    public static string GenerateSchemaId(Type type)
    {
        // Handle generic types
        if (type.IsGenericType)
        {
            var genericTypeName = type.Name.Split(separator: '`')[0];
            var genericArguments = type.GetGenericArguments()
                .Select(selector: GenerateSchemaId) // Recursively simplify generic argument names
                .ToArray();

            return $"{genericTypeName}Of{string.Join(separator: "", value: genericArguments)}";
        }

        // Handle nested types by combining parent names
        var nameParts = new List<string>();
        Type? currentType = type; // Use nullable Type
        while (currentType != null && currentType != typeof(object))
        {
            string part = currentType.Name;
            // Remove generic type indicators (`1, `2, etc.) from the name part
            part = Regex.Replace(input: part, pattern: @"`\d+", replacement: string.Empty);
            // Replace '+' (used for nested types in FullName) with empty string to concatenate smoothly
            part = part.Replace(oldValue: "+", newValue: "");

            if (!string.IsNullOrEmpty(value: part)) // Only add non-empty parts
            {
                nameParts.Insert(index: 0, item: part); // Insert at the beginning to maintain order from outermost to innermost
            }

            currentType = currentType.DeclaringType;
        }

        // Filter out empty parts and join them
        string result = string.Join(separator: "", values: nameParts.Where(predicate: p => !string.IsNullOrEmpty(value: p)));

        // Fallback if the generated result is empty
        if (string.IsNullOrEmpty(value: result))
        {
            // Try to use type.FullName as a fallback, being null-safe
            string? fullName = type.FullName;
            if (fullName != null)
            {
                return fullName.Replace(oldValue: ".", newValue: "_").Replace(oldValue: "+", newValue: "_").Replace(oldValue: "`", newValue: "_").Replace(oldValue: "[", newValue: "").Replace(oldValue: "]", newValue: "");
            }
            else
            {
                // Ultimate fallback if FullName is also null
                return "UnknownSchema";
            }
        }

        return result;
    }
}