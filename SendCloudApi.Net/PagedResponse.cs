using System.Net;

namespace SendCloudApi.Net
{
    public class PagedResponse<T> : ApiResponse<T>
    {
        public PagedResponse(HttpStatusCode statusCode, T data, string rawContent, string next, string prev) 
            : base(statusCode, data, rawContent)
        {
            NextPage = next;
            PreviousPage = prev;
        }
    }
}
