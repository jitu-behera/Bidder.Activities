using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Bidder.Activities.Api.Services
{
    public interface IRequestResponseLogger
    {
        Task LogResponse(MemoryStream memoryStream, Stream originalBodyStream, string routeName);
        Task LogRequest(HttpContext context, string routeName);
        bool IsExcludeLogRoute(string[] excludeLogRoute);
    }


}
