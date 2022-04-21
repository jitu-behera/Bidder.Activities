using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Bidder.Activities.Api.Application.Middleware
{

    public class GzipRequestMiddleware
    {
        private readonly RequestDelegate _next;
        public GzipRequestMiddleware(RequestDelegate next)
        {
            this._next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Headers.Keys.Contains(Gzip.ContentEncodingHeader) && (context.Request.Headers[Gzip.ContentEncodingHeader] == Gzip.ContentEncodingGzip || context.Request.Headers[Gzip.ContentEncodingHeader] == Gzip.ContentEncodingDeflate))
            {
                var contentEncoding = context.Request.Headers[Gzip.ContentEncodingHeader];
                var decompressor = contentEncoding == Gzip.ContentEncodingGzip ? new GZipStream(context.Request.Body, CompressionMode.Decompress, true) : (Stream)new DeflateStream(context.Request.Body, CompressionMode.Decompress, true);
                context.Request.Body = decompressor;
            }
            await _next(context);
        }
    }

    public static class Gzip
    {
        public const string ContentEncodingHeader = "Content-Encoding";
        public const string ContentEncodingGzip = "gzip";
        public const string ContentEncodingDeflate = "deflate";
    }
}