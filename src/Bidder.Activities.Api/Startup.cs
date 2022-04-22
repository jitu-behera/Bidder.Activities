using System;
using Bidder.Activities.Api.Extensions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Bidder.Activities.Api.Domain.Shared;
using Bidder.Activities.Services.Config;
using Bidder.Activities.Api.Application.Filters;
using Bidder.Activities.Api.Application.Middleware;
using Bidder.Activities.CorrelationId;
using Bidder.Activities.Domain.Exceptions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Bidder.Activities.Services;
using RequestResponseLogger.Configuration;

namespace Bidder.Activities.Api
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        protected bool UseAppConfiguration { get; set; } = false;
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        private IConfiguration Configuration { get; }

        private IWebHostEnvironment Environment { get; }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.MapConfigurationToTypedClass(Configuration);
            services.AddExternalServices(Configuration);
            services.AddInternalServices(Configuration, Environment);
            services.AddAzureServices(Configuration);
            services.AddApiVersioning(1);

            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.PropertyNamingPolicy
                    = JsonNamingPolicy.CamelCase;
            });

            services.AddHeaderPropagation();
            services.AddMediatR(typeof(Startup));

            services.AddSwaggerGen(swaggerOptions =>
            {

                swaggerOptions.CustomSchemaIds(x => x.FullName);
                swaggerOptions.SwaggerDoc("v1", new OpenApiInfo { Title = "Widget Activities", Version = "v1" });
                swaggerOptions.OperationFilter<AddRequiredHeaderParameter>();
                swaggerOptions.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description =
                        "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                swaggerOptions.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,

                        },
                        new List<string>()
                    }
                });
            });

            var correlationIdConfiguration = new CorrelationIdConfiguration("BidderActivities", "INLB");
            services.AddCorrelationIdServices(correlationIdConfiguration);

            var loggingConfiguration = new LoggingConfiguration(applicationName: "BidderActivities", applicationVersion: "1.0", isLoggingOn: true, excludeLogRoutes: new string[] { "health" });
            services.AddLoggingProvider(loggingConfiguration);

            services.AddPipelineBehaviors();
            services.AddGzipCompression();

            services.Configure<ApiBehaviorOptions>(o =>
            {
                o.InvalidModelStateResponseFactory = actionContext =>
                    ProcessModelStateErrors(actionContext);
            });

            var regionSettings = new RegionSettings
            {
                ApplicationRegion = Configuration["ApplicationRegion"]
            };
            services.AddSingleton(regionSettings);

            AddJwtConfig(services);
        }

        private IActionResult ProcessModelStateErrors(ActionContext actionContext)
        {
            var response = new ModelBindingValidationError
            {
                ValidationResults = new List<ValidationError>()
            };
            foreach (var key in actionContext.ModelState.Keys)
            {
                if (!actionContext.ModelState[key].Errors.Any()) continue;
                var errorMessages = actionContext.ModelState[key].Errors.Select(x => x.ErrorMessage);
                var description = string.Join(".", errorMessages);
                response.ValidationResults.Add(new ValidationError(100, "ERROR_MISSING_DATA", description, key));
            }

            return new BadRequestObjectResult(response);
        }
        private void AddJwtConfig(IServiceCollection services)
        {
            // Get options from app settings

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = Literals.TokenValidation.ClaimsIssuer,
                ValidateAudience = true,
                ValidAudience = Literals.TokenValidation.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = GetSecurityKey(),
                RequireExpirationTime = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(configureOptions =>
            {
                configureOptions.ClaimsIssuer = Literals.TokenValidation.ClaimsIssuer;
                configureOptions.TokenValidationParameters = tokenValidationParameters;
                configureOptions.SaveToken = true;
            });
        }

        private SymmetricSecurityKey GetSecurityKey()
        {
            if (Environment.EnvironmentName == "Development")
            {
                var signInSecretKey = Configuration.GetValue<string>("SecretKey");
                return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(signInSecretKey));
            }

            var securitySettings = new SecurityKeySettings();
            Configuration.Bind(nameof(SecurityKeySettings), securitySettings);
            var symmetricSecurityKey = new SecurityKeyRepository(securitySettings).SecurityKey;
            return symmetricSecurityKey;
        }

        public virtual void Configure(IApplicationBuilder app)
        {
            app.UseMiddleware<GzipRequestMiddleware>();
            // Enable response compression (This must be called before any middleware that compresses responses).
            app.UseResponseCompression();

            app.UseMiddleware<ExceptionMiddleware>();
            app.UseMiddleware<CookieAuthMiddleware>();

            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();

            app.RegisterSwagger();

            app.RegisterMiddlewares();
            if (UseAppConfiguration)
                app.UseAzureAppConfiguration();

            app.UseHttpsRedirection();
            app.UseEndpoints(endpoints => { endpoints.MapControllers().RequireAuthorization(); });

        }

        /// <summary>
        /// Swagger filter that enables the required parameter option on headers.
        /// These headers are specified in the configuration.
        /// </summary>
        private class AddRequiredHeaderParameter : IOperationFilter
        {
            private readonly IOptions<Headers> _options;

            public AddRequiredHeaderParameter(IOptions<Headers> options)
            {
                _options = options;
            }

            public void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
                operation.Parameters ??= new List<OpenApiParameter>();

                foreach (var key in _options.Value.Request)
                {
                    operation.Parameters.Add(new OpenApiParameter
                    {
                        Name = key,
                        In = ParameterLocation.Header,
                        Schema = new OpenApiSchema { Type = "string" },
                        Required = true,
                        Example = new OpenApiString("1")
                    });
                }
            }
        }
    }
}