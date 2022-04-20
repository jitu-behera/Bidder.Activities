using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;
using Bidder.Activities.Api.Application.Middleware;
using Bidder.Activities.HttpHeaders;

namespace Bidder.Activities.Api.Extensions
{
    /// <summary>
    /// Extension methods on IApplicationBuilder.
    /// Other required extension methods for IApplicationBuilder can be created in this class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class ApplicationBuilderExtensions
    {
        private static readonly string[] ExcludeLogRoutes = { "health" };
       
        /// <summary>
        /// Register all custom middleware for IApplicationBuilder here.
        /// </summary>
        /// <param name="app"></param>
        internal static void RegisterMiddlewares(this IApplicationBuilder app)
        {
            

            app.UseMiddleware<SetCorrelationIdMiddleware>();

            app.UseMiddleware<HeaderValidationMiddleware>();
           
            app.UseWhen(IsExcludeLogRoute, appBuilder =>
            {
                appBuilder.UseMiddleware<RequestResponseLoggingMiddleware>();
            });

        }
        /// <summary>
        /// Exclude route healthcheck and batch for request response logging 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private static bool IsExcludeLogRoute(HttpContext context)
        {
            foreach (string route in ExcludeLogRoutes)
            {
                if (context.Request.Path.ToString().ToLower().EndsWith(route))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Register swagger and its options here.
        /// </summary>
        /// <param name="app"></param>
        internal static void RegisterSwagger(this IApplicationBuilder app)
        {
            // Enable middleware to serve generated Swagger as a json endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui.
            app.UseSwaggerUI(swaggerOptions =>
            {
                swaggerOptions.SwaggerEndpoint("/swagger/v1/swagger.json", "Bidder.Activities.Api v1");
                swaggerOptions.DisplayRequestDuration();
            });
        }
    }
}