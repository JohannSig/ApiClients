using System.Net;

namespace FrozenForge.Apis
{
    public class ApiResponse
    {
        public string ReasonPhrase { get; set; }
                
        public HttpStatusCode? StatusCode { get; set; }

        public object Body { get; set; }

        public bool IsValid => (int?)StatusCode is >= 200 and < 300;
    }

    public class ApiResponse<TData>
        : ApiResponse
    {
        public TData Data { get; set; }
    }
}
