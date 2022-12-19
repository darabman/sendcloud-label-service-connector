using System.Net;

namespace SendCloudApi.Net
{
    public class ApiResponse<T>
    {
        public HttpStatusCode StatusCode { get; }

        public T Data { get; }

        public string RawContent { get; }

        public string? NextPage { get; set; } = null;

        public string? PreviousPage { get; set; } = null;

        public ApiResponse(HttpStatusCode statusCode, T data, string rawContent)
        {
            StatusCode = statusCode;
            Data = data;
            RawContent = rawContent;
        }

        public ApiResponse(HttpStatusCode statusCode, T data) : this(statusCode, data, null)
        {
        }
    }
}
