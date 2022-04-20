using Bidder.Activities.Api.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Polly.CircuitBreaker;
using Polly.Timeout;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Bidder.Activities.Services;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Bidder.Activities.Api.Application.Middleware
{
    /// <summary>
    /// Global exception handling for inbuilt as well as custom exceptions.
    /// All the exceptions thrown from anywhere after the application starts will be handled from here globally using HandleExceptionAsync().
    /// This file can be extended to handle further exceptions as required.
    /// </summary>
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
      
        private readonly ITelemetryLogger _telemetryLogger;

        public ExceptionMiddleware(RequestDelegate next, ITelemetryLogger telemetryLogger)
        {
          
            _telemetryLogger = telemetryLogger;
            _next = next;
        
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _telemetryLogger.LogException(exception);

            // Here we can extend the implementation to add more type of exceptions if needed
            switch (exception)
            {
                case ValidationException validationException:
                    var validationsErrors = JsonSerializer.Serialize(validationException.Errors);
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                    context.Response.Headers.Add("X-Status-Reason", "Validation failed");
                    return context.Response.WriteAsync(validationsErrors);
                case HttpRequestException _:
                case BrokenCircuitException _:
                case TimeoutRejectedException _:
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                    return context.Response.WriteAsync(new ErrorResult
                    {
                        StatusCode = context.Response.StatusCode,
                        Message = ResourceReader.ReadValue("ServiceUnavailable")
                    }.ToString());
                case HeaderValidationException _:
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return context.Response.WriteAsync(new ErrorResult
                    {
                        StatusCode = context.Response.StatusCode,
                        Message = ResourceReader.ReadValue("InsufficientHeaders")
                    }.ToString());
                case TransientException transientException:
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                    context.Response.Headers.Add("Retry-After", transientException.RetryAfterSeconds.ToString());
                    AddHeaders(context, transientException.HeaderDictionary);
                    return context.Response.WriteAsync(new ErrorResult
                    {
                        StatusCode = context.Response.StatusCode,
                        Message = ResourceReader.ReadValue("TransientMessage")
                    }.ToString());
                case NonTransientException nonTransientException:
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    AddHeaders(context, nonTransientException.HeaderDictionary);
                    return context.Response.WriteAsync(new ErrorResult
                    {
                        StatusCode = context.Response.StatusCode,
                        Message = ResourceReader.ReadValue("NonTransientMessage")
                    }.ToString());

                default:
                  
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return context.Response.WriteAsync(new ErrorResult
                    {
                        StatusCode = context.Response.StatusCode,
                        Message = ResourceReader.ReadValue("NonTransientMessage")
                    }.ToString());
            }
        }

        /// <summary>
        /// Adds headers to the HttpContext from HeaderDictionary which are added by while code execution
        /// </summary>
        /// <param name="context"></param>
        /// <param name="headerDictionary"></param>
        private static void AddHeaders(HttpContext context, IReadOnlyDictionary<string, string> headerDictionary)
        {
            if (headerDictionary != null)
            {
                foreach (var (key, value) in headerDictionary)
                {
                    context.Response.Headers.Add(key, value);
                }
            }
        }
    }

    /// <summary>
    /// Static class to read string resources
    /// </summary>
    internal static class ResourceReader
    {
        static ResourceReader()
        {
            Resources = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText("Resources.json"));
        }

        private static Dictionary<string, string> Resources { get; }

        public static string ReadValue(string key, params string[] placeHolders)
        {
            var textResource = Resources[key];

            if (placeHolders != null && placeHolders.Any())
            {
                textResource = string.Format(textResource, placeHolders);
            }

            return textResource;
        }
    }
}